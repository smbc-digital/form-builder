namespace form_builder.Services.BookingService;

public class BookingService(IDistributedCacheWrapper distributedCache,
    IPageHelper pageHelper,
    IEnumerable<IBookingProvider> bookingProviders,
    IPageFactory pageFactory,
    IMappingService mappingService,
    IWebHostEnvironment environment,
    ISchemaFactory schemaFactory,
    ISessionHelper sessionHelper,
    IHashUtil hashUtil,
    IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
    IHttpContextAccessor httpContextAccessor,
    IEnumerable<ITagParser> tagParsers)
    : IBookingService
{
    private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;

    public async Task<BookingProcessEntity> Get(string baseUrl, Page currentPage, string cacheKey)
    {
        var bookingElement = currentPage.Elements
            .First(element => element.Type
                .Equals(EElementType.Booking));

        List<AvailabilityDayResponse> appointmentTimes = new();

        var appointmentType = bookingElement.Properties.AppointmentTypes
            .GetAppointmentTypeForEnvironment(environment.EnvironmentName);

        var bookingInformationCacheKey = $"{bookingElement.Properties.QuestionId}:{appointmentType.AppointmentId}:" +
                                         $"{bookingElement.Properties.LimitNextAvailableFromDate}:" +
                                         $"{appointmentType.OptionalResources.CreateKeyFromResources()}:" +
                                         $"{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";

        var cachedAnswers = distributedCache.GetString(cacheKey);
        FormAnswers convertedAnswers = new();
        if (cachedAnswers is not null)
        {
            convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            if (appointmentType.NeedsAppointmentIdMapping)
                mappingService.MapAppointmentId(appointmentType, convertedAnswers);

            if (convertedAnswers.FormData.ContainsKey(bookingInformationCacheKey))
            {
                var cachedBookingInformation = JsonConvert
                    .DeserializeObject<BookingInformation>(convertedAnswers.FormData[bookingInformationCacheKey].ToString());

                if (appointmentType.AppointmentId.Equals(cachedBookingInformation.AppointmentTypeId))
                    return new BookingProcessEntity { BookingInfo = new() { cachedBookingInformation } };
            }
        }

        var bookingProvider = bookingProviders.Get(bookingElement.Properties.BookingProvider);

        var nextAvailability = await RetrieveNextAvailability(bookingElement, bookingProvider, appointmentType);

        if (nextAvailability.BookingHasNoAvailableAppointments)
            return new BookingProcessEntity { BookingHasNoAvailableAppointments = true };

        appointmentTimes = await bookingProvider.GetAvailability(new AvailabilityRequest
        {
            StartDate = nextAvailability.DayResponse.Date,
            EndDate = nextAvailability.DayResponse.Date.LastDayOfTheMonth(),
            AppointmentId = appointmentType.AppointmentId,
            OptionalResources = appointmentType.OptionalResources
        });

        BookingInformation bookingInformation = new()
        {
            AppointmentTypeId = appointmentType.AppointmentId,
            Appointments = appointmentTimes.Where(appointment => appointment.HasAvailableAppointment).ToList(),
            CurrentSearchedMonth = new DateTime(nextAvailability.DayResponse.Date.Year, nextAvailability.DayResponse.Date.Month, 1),
            FirstAvailableMonth = new DateTime(nextAvailability.DayResponse.Date.Year, nextAvailability.DayResponse.Date.Month, 1),
            IsFullDayAppointment = nextAvailability.DayResponse.IsFullDayAppointment
        };

        if (nextAvailability.DayResponse.IsFullDayAppointment)
        {
            bookingInformation.AppointmentStartTime = DateTime.Today.Add(nextAvailability.DayResponse.AppointmentTimes.First().StartTime);
            bookingInformation.AppointmentEndTime = DateTime.Today.Add(nextAvailability.DayResponse.AppointmentTimes.First().EndTime);
        }

        pageHelper.SaveFormData(bookingInformationCacheKey, bookingInformation, cacheKey, baseUrl);

        return new BookingProcessEntity { BookingInfo = new() { bookingInformation } };
    }

    public async Task<ProcessRequestEntity> ProcessBooking(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string cacheKey, string path)
    {
        var bookingElement = (Booking)currentPage.Elements.First(_ => _.Type.Equals(EElementType.Booking));
        viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

        switch (subPath)
        {
            case BookingConstants.CHECK_YOUR_BOOKING:
                return await ProcessCheckYourBooking(viewModel, currentPage, baseForm, cacheKey, path, bookingElement);
            default:
                return await ProcessDateAndTime(viewModel, currentPage, baseForm, cacheKey, path, bookingElement);
        }
    }

    public async Task ProcessMonthRequest(Dictionary<string, object> viewModel, string form, string path)
    {
        var baseForm = await schemaFactory.Build(form);

        if (baseForm is null)
            throw new ApplicationException($"Requested form '{form}' could not be found.");

        var currentPage = baseForm.GetPage(pageHelper, path, form);
        if (currentPage is null)
            throw new ApplicationException($"Requested path '{path}' object could not be found for form '{form}'");

        string browserSessionId = sessionHelper.GetBrowserSessionId();

        if (string.IsNullOrEmpty(browserSessionId))
            throw new ApplicationException("BookingService::ProcessMonthRequest Session has expired");

        string cacheKey = $"{form}::{browserSessionId}";
        var cachedAnswers = distributedCache.GetString(cacheKey);

        if (cachedAnswers is null)
            throw new ApplicationException("BookingService::ProcessMonthRequest, Session data is null");

        var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        await schemaFactory.TransformPage(currentPage, convertedAnswers);
        foreach (var tagParser in tagParsers)
        {
            await tagParser.Parse(currentPage, convertedAnswers, baseForm);
        }

        if (!viewModel.ContainsKey(BookingConstants.BOOKING_MONTH_REQUEST))
            throw new ApplicationException("BookingService::ProcessMonthRequest, request for appointment did not contain requested month");

        var requestedMonth = DateTime.Parse(viewModel[BookingConstants.BOOKING_MONTH_REQUEST].ToString());

        var currentDate = DateTime.Now;
        var bookingElement = (Booking)currentPage.Elements
            .First(element => element.Type.
                Equals(EElementType.Booking));

        if (requestedMonth.Month.Equals(currentDate.Month) && requestedMonth.Year.Equals(currentDate.Year))
            requestedMonth = currentDate;

        if (requestedMonth > new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(bookingElement.Properties.SearchPeriod))
            throw new ApplicationException("BookingService::ProcessMonthRequest, Invalid request for appointment search, Start date provided is after allowed search period");

        if (requestedMonth < currentDate)
            throw new ApplicationException("BookingService::ProcessMonthRequest, Invalid request for appointment search, Start date provided is before today");

        var appointmentType = bookingElement.Properties.AppointmentTypes
            .GetAppointmentTypeForEnvironment(environment.EnvironmentName);

        if (appointmentType.NeedsAppointmentIdMapping)
            mappingService.MapAppointmentId(appointmentType, convertedAnswers);

        var bookingInformationCacheKey = $"{bookingElement.Properties.QuestionId}:{appointmentType.AppointmentId}:" +
                                         $"{bookingElement.Properties.LimitNextAvailableFromDate}:" +
                                         $"{appointmentType.OptionalResources.CreateKeyFromResources()}:" +
                                         $"{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";

        var appointmentTimes = await bookingProviders.Get(bookingElement.Properties.BookingProvider)
            .GetAvailability(new AvailabilityRequest
            {
                StartDate = requestedMonth.Date,
                EndDate = requestedMonth.Date.LastDayOfTheMonth(),
                AppointmentId = appointmentType.AppointmentId,
                OptionalResources = appointmentType.OptionalResources
            });

        if (!convertedAnswers.FormData.ContainsKey(bookingInformationCacheKey))
            throw new ApplicationException($"BookingService::ProcessMonthRequest, The key {bookingInformationCacheKey} is not stored in FormData");

        var cachedBookingInformation = JsonConvert.DeserializeObject<BookingInformation>(convertedAnswers.FormData[bookingInformationCacheKey].ToString());
        BookingInformation bookingInformation = new()
        {
            AppointmentTypeId = appointmentType.AppointmentId,
            Appointments = appointmentTimes.Where(appointment => appointment.HasAvailableAppointment).ToList(),
            CurrentSearchedMonth = requestedMonth,
            FirstAvailableMonth = cachedBookingInformation.FirstAvailableMonth,
            IsFullDayAppointment = cachedBookingInformation.IsFullDayAppointment,
            AppointmentEndTime = cachedBookingInformation.AppointmentEndTime,
            AppointmentStartTime = cachedBookingInformation.AppointmentStartTime
        };

        pageHelper.SaveFormData(bookingInformationCacheKey, bookingInformation, cacheKey, baseForm.BaseURL);
    }

    public async Task<CancelledAppointmentInformation> ValidateCancellationRequest(string formName, Guid bookingGuid, string hash)
    {
        if (!hashUtil.Check(bookingGuid.ToString(), hash))
            throw new ApplicationException($"BookingService::ValidateCancellationRequest,Booking Guid does not match hash, unable to verify request integrity");

        var formSchema = await schemaFactory.Build(formName);

        if (formSchema is null)
            throw new ApplicationException($"BookingService::ValidateCancellationRequest, Provided formname '{formName}' is not valid and cannot be resolved");

        var provider = formSchema.Pages
            .Where(page => page.Elements is not null)
            .SelectMany(page => page.Elements)
            .First(element => element.Type.Equals(EElementType.Booking)).Properties.BookingProvider;

        var bookingInformation = await bookingProviders.Get(provider).GetBooking(bookingGuid);

        if (!bookingInformation.CanCustomerCancel)
            throw new BookingCannotBeCancelledException($"BookingSerivice::ValidateCancellationRequest, booking: {bookingGuid} specified it can not longer be cancelled");

        var envStartPageUrl = $"https://{httpContextAccessor.HttpContext.Request.Host}{environment.EnvironmentName.ToReturnUrlPrefix()}/{formName}/{formSchema.StartPageUrl}";

        return new CancelledAppointmentInformation
        {
            FormName = formSchema.FormName,
            StartPageUrl = formSchema.StartPageUrl.StartsWith("https://") || formSchema.StartPageUrl.StartsWith("http://") ? formSchema.StartPageUrl : envStartPageUrl,
            BaseURL = formSchema.BaseURL,
            BookingDate = bookingInformation.Date,
            Id = bookingInformation.Id,
            StartTime = bookingInformation.StartTime,
            EndTime = bookingInformation.EndTime,
            IsFullday = bookingInformation.IsFullDay,
            Hash = hash,
        };
    }

    public async Task Cancel(string formName, Guid bookingGuid, string hash)
    {
        if (!hashUtil.Check(bookingGuid.ToString(), hash))
            throw new ApplicationException($"BookingService::Cancel, Booking Guid does not match hash, unable to verify request integrity");

        var formSchema = await schemaFactory.Build(formName);

        if (formSchema is null)
            throw new ApplicationException($"BookingService::Cancel, Provided formname '{formName}' is not valid and cannot be resolved");

        var provider = formSchema.Pages
            .Where(page => page.Elements is not null)
            .SelectMany(page => page.Elements)
            .First(element => element.Type.Equals(EElementType.Booking)).Properties.BookingProvider;

        await bookingProviders.Get(provider).Cancel(bookingGuid);
    }

    private async Task<BookingNextAvailabilityEntity> RetrieveNextAvailability(IElement bookingElement, IBookingProvider bookingProvider, AppointmentType appointmentType)
    {
        var bookingNextAvailabilityCachedKey = $"{bookingElement.Properties.BookingProvider}-{bookingElement.Properties.QuestionId}-" +
                                               $"{bookingElement.Properties.LimitNextAvailableFromDate}-" +
                                               $"{appointmentType.AppointmentId}{appointmentType.OptionalResources.CreateKeyFromResources()}";

        var bookingNextAvailabilityCachedResponse = distributedCache.GetString(bookingNextAvailabilityCachedKey);
        if (bookingNextAvailabilityCachedResponse is not null)
            return JsonConvert.DeserializeObject<BookingNextAvailabilityEntity>(bookingNextAvailabilityCachedResponse);

        var hasLimitProperties = !string.IsNullOrEmpty(bookingElement.Properties.LimitNextAvailableFromDate) &&
                                 bookingElement.Properties.LimitNextAvailableByDays > 0;

        var startDate = hasLimitProperties
            ? DateTime.Parse(bookingElement.Properties.LimitNextAvailableFromDate).AddDays(bookingElement.Properties.LimitNextAvailableByDays)
            : DateTime.Now;

        AvailabilityDayResponse nextAvailability;
        BookingNextAvailabilityEntity result;
        try
        {
            nextAvailability = await bookingProvider
                .NextAvailability(new AvailabilityRequest
                {
                    StartDate = startDate,
                    EndDate = startDate.AddMonths(bookingElement.Properties.SearchPeriod),
                    AppointmentId = appointmentType.AppointmentId,
                    OptionalResources = appointmentType.OptionalResources
                });
        }
        catch (BookingNoAvailabilityException)
        {
            result = new BookingNextAvailabilityEntity { BookingHasNoAvailableAppointments = true };
            await distributedCache.SetStringAbsoluteAsync(bookingNextAvailabilityCachedKey, JsonConvert.SerializeObject(result), _distributedCacheExpirationConfiguration.BookingNoAppointmentsAvailable);
            return result;
        }

        result = new BookingNextAvailabilityEntity { DayResponse = nextAvailability };
        await distributedCache.SetStringAbsoluteAsync(bookingNextAvailabilityCachedKey, JsonConvert.SerializeObject(result), _distributedCacheExpirationConfiguration.Booking);
        return result;
    }

    private async Task<ProcessRequestEntity> ProcessDateAndTime(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string cacheKey, string path, Booking element)
    {
        if (!currentPage.IsValid)
        {
            var cachedAnswers = distributedCache.GetString(cacheKey);

            var convertedAnswers = cachedAnswers is null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            var appointmentType = element.Properties.AppointmentTypes
                .GetAppointmentTypeForEnvironment(environment.EnvironmentName);

            await schemaFactory.TransformPage(currentPage, convertedAnswers);
            foreach (var tagParser in tagParsers)
            {
                await tagParser.Parse(currentPage, convertedAnswers, baseForm);
            }

            var bookingInformationCacheKey = $"{element.Properties.QuestionId}:{appointmentType.AppointmentId}:" +
                                             $"{element.Properties.LimitNextAvailableFromDate}:" +
                                             $"{appointmentType.OptionalResources.CreateKeyFromResources()}:" +
                                             $"{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";

            if (!convertedAnswers.FormData.ContainsKey(bookingInformationCacheKey))
                throw new ApplicationException($"BookingService::ProcessDateAndTime, The key {bookingInformationCacheKey} is not stored in FormData");

            var cachedBookingInformation = JsonConvert.DeserializeObject<BookingInformation>(convertedAnswers.FormData[$"{bookingInformationCacheKey}"].ToString());
            var bookingInformation = new List<object> { cachedBookingInformation };
            var model = await pageFactory.Build(currentPage, viewModel, baseForm, cacheKey, null, bookingInformation);

            return new ProcessRequestEntity
            {
                Page = currentPage,
                ViewModel = model
            };
        }

        if (!element.Properties.CheckYourBooking)
        {
            await ReserveAppointment(element, viewModel, baseForm.BaseURL, cacheKey);

            pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);

        return new ProcessRequestEntity
        {
            RedirectToAction = true,
            RedirectAction = "Index",
            RouteValues = new
            {
                form = baseForm.BaseURL,
                path,
                subPath = BookingConstants.CHECK_YOUR_BOOKING
            }
        };
    }

    private async Task<ProcessRequestEntity> ProcessCheckYourBooking(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string cacheKey, string path, Booking element)
    {
        await ReserveAppointment(element, viewModel, baseForm.BaseURL, cacheKey);

        pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);

        return new ProcessRequestEntity { Page = currentPage };
    }

    private async Task<Guid> ReserveAppointment(Booking bookingElement, Dictionary<string, dynamic> viewModel, string form, string cacheKey)
    {
        var reservedBookingId = bookingElement.ReservedIdQuestionId;
        var reservedBookingAppointmentId = bookingElement.ReservedAppointmentIdQuestionId;

        var reservedBookingDate = bookingElement.ReservedDateQuestionId;
        var reservedBookingStartTime = bookingElement.ReservedStartTimeQuestionId;
        var reservedBookingEndTime = bookingElement.ReservedEndTimeQuestionId;

        var currentlySelectedBookingDate = bookingElement.DateQuestionId;
        var currentlySelectedBookingStartTime = bookingElement.StartTimeQuestionId;
        var currentlySelectedBookingEndTime = bookingElement.EndTimeQuestionId;

        var bookingRequest = await mappingService.MapBookingRequest(cacheKey, bookingElement, viewModel, form);
        var location = await GetReservedBookingLocation(bookingElement, bookingRequest);
        viewModel.Add(bookingElement.AppointmentLocation, location);

        if (viewModel.ContainsKey(reservedBookingId) && !string.IsNullOrEmpty((string)viewModel[reservedBookingId]))
        {
            var currentSelectedAppointmentId = bookingRequest.AppointmentId.ToString();
            var currentSelectedDate = (string)viewModel[currentlySelectedBookingDate];
            var currentSelectedStartTime = (string)viewModel[currentlySelectedBookingStartTime];
            var currentSelectedEndTime = (string)viewModel[currentlySelectedBookingEndTime];
            var previouslyReservedAppointmentId = (string)viewModel[reservedBookingAppointmentId];
            var previouslyReservedAppointmentDate = (string)viewModel[reservedBookingDate];
            var previouslyReservedAppointmentStartTime = (string)viewModel[reservedBookingStartTime];
            var previouslyReservedAppointmentEndTime = (string)viewModel[reservedBookingEndTime];

            if (currentSelectedDate.Equals(previouslyReservedAppointmentDate)
                && currentSelectedStartTime.Equals(previouslyReservedAppointmentStartTime)
                && currentSelectedEndTime.Equals(previouslyReservedAppointmentEndTime)
                && currentSelectedAppointmentId.Equals(previouslyReservedAppointmentId))
                return Guid.Parse(viewModel[reservedBookingId]);
        }

        viewModel.Remove(reservedBookingId);
        viewModel.Remove(reservedBookingAppointmentId);
        viewModel.Remove(reservedBookingDate);
        viewModel.Remove(reservedBookingStartTime);
        viewModel.Remove(reservedBookingEndTime);

        var result = await bookingProviders.Get(bookingElement.Properties.BookingProvider)
            .Reserve(bookingRequest);

        viewModel.Add(reservedBookingAppointmentId, bookingRequest.AppointmentId.ToString());
        viewModel.Add(reservedBookingDate, viewModel[currentlySelectedBookingDate]);
        viewModel.Add(reservedBookingStartTime, viewModel[currentlySelectedBookingStartTime]);
        viewModel.Add(reservedBookingEndTime, viewModel[currentlySelectedBookingEndTime]);
        viewModel.Add(reservedBookingId, result);

        return result;
    }

    private async Task<string> GetReservedBookingLocation(Booking bookingElement, BookingRequest bookingRequest)
    {
        var bookingProvider = bookingProviders.Get(bookingElement.Properties.BookingProvider);
        var location = await bookingProvider.GetLocation(new LocationRequest
        {
            AppointmentId = bookingRequest.AppointmentId,
            OptionalResources = bookingRequest.OptionalResources
        });

        if (string.IsNullOrEmpty(location))
        {
            return string.IsNullOrEmpty(bookingRequest.Customer.Address) ? string.Empty : bookingRequest.Customer.Address.ConvertAddressToTitleCase();
        }

        return location.ConvertAddressToTitleCase();
    }
}