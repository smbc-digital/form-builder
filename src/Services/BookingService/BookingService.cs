using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Booking;
using form_builder.Models.Elements;
using form_builder.Providers.Booking;
using form_builder.Providers.StorageProvider;
using form_builder.Services.BookingService.Entities;
using form_builder.Services.MappingService;
using form_builder.Services.PageService.Entities;
using form_builder.TagParsers;
using form_builder.Utils.Extensions;
using form_builder.Utils.Hash;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Request;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Response;

namespace form_builder.Services.BookingService
{
    public class BookingService : IBookingService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IPageHelper _pageHelper;
        private readonly IEnumerable<IBookingProvider> _bookingProviders;
        private readonly IPageFactory _pageFactory;
        private readonly IMappingService _mappingService;
        private readonly IWebHostEnvironment _environment;
        private readonly ISchemaFactory _schemaFactory;
        private readonly ISessionHelper _sessionHelper;
        private readonly IHashUtil _hashUtil;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<ITagParser> _tagParsers;

        public BookingService(
            IDistributedCacheWrapper distributedCache,
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
        {
            _distributedCache = distributedCache;
            _pageHelper = pageHelper;
            _bookingProviders = bookingProviders;
            _pageFactory = pageFactory;
            _mappingService = mappingService;
            _environment = environment;
            _schemaFactory = schemaFactory;
            _sessionHelper = sessionHelper;
            _hashUtil = hashUtil;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _httpContextAccessor = httpContextAccessor;
            _tagParsers = tagParsers;
        }

        public async Task<BookingProcessEntity> Get(string baseUrl, Page currentPage, string sessionGuid)
        {
            var bookingElement = currentPage.Elements
                .First(element => element.Type
                    .Equals(EElementType.Booking));

            List<AvailabilityDayResponse> appointmentTimes = new();

            var appointmentType = bookingElement.Properties.AppointmentTypes
                .GetAppointmentTypeForEnvironment(_environment.EnvironmentName);

            var bookingInformationCacheKey = $"{bookingElement.Properties.QuestionId}:{appointmentType.AppointmentId}:" +
                                             $"{bookingElement.Properties.LimitNextAvailableFromDate}:" +
                                             $"{appointmentType.OptionalResources.CreateKeyFromResources()}:" +
                                             $"{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";

            var cachedAnswers = _distributedCache.GetString(sessionGuid);
            FormAnswers convertedAnswers = new();
            if (cachedAnswers is not null)
            {
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                if (appointmentType.NeedsAppointmentIdMapping)
                    _mappingService.MapAppointmentId(appointmentType, convertedAnswers);

                if (convertedAnswers.FormData.ContainsKey(bookingInformationCacheKey))
                {
                    var cachedBookingInformation = JsonConvert
                        .DeserializeObject<BookingInformation>(convertedAnswers.FormData[bookingInformationCacheKey].ToString());

                    if (appointmentType.AppointmentId.Equals(cachedBookingInformation.AppointmentTypeId))
                        return new BookingProcessEntity { BookingInfo = new() { cachedBookingInformation } };
                }
            }

            var bookingProvider = _bookingProviders.Get(bookingElement.Properties.BookingProvider);

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

            _pageHelper.SaveFormData(bookingInformationCacheKey, bookingInformation, sessionGuid, baseUrl);

            return new BookingProcessEntity { BookingInfo = new() { bookingInformation } };
        }

        public async Task<ProcessRequestEntity> ProcessBooking(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            var bookingElement = (Booking)currentPage.Elements.First(_ => _.Type.Equals(EElementType.Booking));
            viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

            switch (subPath)
            {
                case BookingConstants.CHECK_YOUR_BOOKING:
                    return await ProcessCheckYourBooking(viewModel, currentPage, baseForm, guid, path, bookingElement);
                default:
                    return await ProcessDateAndTime(viewModel, currentPage, baseForm, guid, path, bookingElement);
            }
        }

        public async Task ProcessMonthRequest(Dictionary<string, object> viewModel, string form, string path)
        {
            var baseForm = await _schemaFactory.Build(form);

            if (baseForm is null)
                throw new ApplicationException($"Requested form '{form}' could not be found.");

            var currentPage = baseForm.GetPage(_pageHelper, path);
            if (currentPage is null)
                throw new ApplicationException($"Requested path '{path}' object could not be found for form '{form}'");

            var guid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(guid))
                throw new ApplicationException("BookingService::ProcessMonthRequest Session has expired");
            var cachedAnswers = _distributedCache.GetString(guid);

            if (cachedAnswers is null)
                throw new ApplicationException("BookingService::ProcessMonthRequest, Session data is null");

            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            await _schemaFactory.TransformPage(currentPage, convertedAnswers);
            foreach (var tagParser in _tagParsers)
            {
                await tagParser.Parse(currentPage, convertedAnswers);
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
                .GetAppointmentTypeForEnvironment(_environment.EnvironmentName);

            if (appointmentType.NeedsAppointmentIdMapping)
                _mappingService.MapAppointmentId(appointmentType, convertedAnswers);

            var bookingInformationCacheKey = $"{bookingElement.Properties.QuestionId}:{appointmentType.AppointmentId}:" +
                                             $"{bookingElement.Properties.LimitNextAvailableFromDate}:" +
                                             $"{appointmentType.OptionalResources.CreateKeyFromResources()}:" +
                                             $"{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";

            var appointmentTimes = await _bookingProviders.Get(bookingElement.Properties.BookingProvider)
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

            _pageHelper.SaveFormData(bookingInformationCacheKey, bookingInformation, guid, baseForm.BaseURL);
        }

        public async Task<CancelledAppointmentInformation> ValidateCancellationRequest(string formName, Guid bookingGuid, string hash)
        {
            if (!_hashUtil.Check(bookingGuid.ToString(), hash))
                throw new ApplicationException($"BookingService::ValidateCancellationRequest,Booking guid does not match hash, unable to verify request integrity");

            var formSchema = await _schemaFactory.Build(formName);

            if (formSchema is null)
                throw new ApplicationException($"BookingService::ValidateCancellationRequest, Provided formname '{formName}' is not valid and cannot be resolved");

            var provider = formSchema.Pages
                .Where(page => page.Elements is not null)
                .SelectMany(page => page.Elements)
                .First(element => element.Type.Equals(EElementType.Booking)).Properties.BookingProvider;

            var bookingInformation = await _bookingProviders.Get(provider).GetBooking(bookingGuid);

            if (!bookingInformation.CanCustomerCancel)
                throw new BookingCannotBeCancelledException($"BookingSerivice::ValidateCancellationRequest, booking: {bookingGuid} specified it can not longer be cancelled");

            var envStartPageUrl = $"https://{_httpContextAccessor.HttpContext.Request.Host}{_environment.EnvironmentName.ToReturnUrlPrefix()}/{formName}/{formSchema.StartPageUrl}";

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
            if (!_hashUtil.Check(bookingGuid.ToString(), hash))
                throw new ApplicationException($"BookingService::Cancel, Booking guid does not match hash, unable to verify request integrity");

            var formSchema = await _schemaFactory.Build(formName);

            if (formSchema is null)
                throw new ApplicationException($"BookingService::Cancel, Provided formname '{formName}' is not valid and cannot be resolved");

            var provider = formSchema.Pages
                .Where(page => page.Elements is not null)
                .SelectMany(page => page.Elements)
                .First(element => element.Type.Equals(EElementType.Booking)).Properties.BookingProvider;

            await _bookingProviders.Get(provider).Cancel(bookingGuid);
        }

        private async Task<BookingNextAvailabilityEntity> RetrieveNextAvailability(IElement bookingElement, IBookingProvider bookingProvider, AppointmentType appointmentType)
        {
            var bookingNextAvailabilityCachedKey = $"{bookingElement.Properties.BookingProvider}-{bookingElement.Properties.QuestionId}-" +
                                                   $"{bookingElement.Properties.LimitNextAvailableFromDate}-" +
                                                   $"{appointmentType.AppointmentId}{appointmentType.OptionalResources.CreateKeyFromResources()}";

            var bookingNextAvailabilityCachedResponse = _distributedCache.GetString(bookingNextAvailabilityCachedKey);
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
                await _distributedCache.SetStringAsync(bookingNextAvailabilityCachedKey, JsonConvert.SerializeObject(result), _distributedCacheExpirationConfiguration.BookingNoAppointmentsAvailable);
                return result;
            }

            result = new BookingNextAvailabilityEntity { DayResponse = nextAvailability };
            await _distributedCache.SetStringAsync(bookingNextAvailabilityCachedKey, JsonConvert.SerializeObject(result), _distributedCacheExpirationConfiguration.Booking);
            return result;
        }

        private async Task<ProcessRequestEntity> ProcessDateAndTime(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path, Booking element)
        {
            if (!currentPage.IsValid)
            {
                var cachedAnswers = _distributedCache.GetString(guid);

                var convertedAnswers = cachedAnswers is null
                    ? new FormAnswers { Pages = new List<PageAnswers>() }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                var appointmentType = element.Properties.AppointmentTypes
                    .GetAppointmentTypeForEnvironment(_environment.EnvironmentName);

                await _schemaFactory.TransformPage(currentPage, convertedAnswers);
                foreach (var tagParser in _tagParsers)
                {
                    await tagParser.Parse(currentPage, convertedAnswers);
                }

                var bookingInformationCacheKey = $"{element.Properties.QuestionId}:{appointmentType.AppointmentId}:" +
                                                 $"{element.Properties.LimitNextAvailableFromDate}:" +
                                                 $"{appointmentType.OptionalResources.CreateKeyFromResources()}:" +
                                                 $"{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";

                if (!convertedAnswers.FormData.ContainsKey(bookingInformationCacheKey))
                    throw new ApplicationException($"BookingService::ProcessDateAndTime, The key {bookingInformationCacheKey} is not stored in FormData");

                var cachedBookingInformation = JsonConvert.DeserializeObject<BookingInformation>(convertedAnswers.FormData[$"{bookingInformationCacheKey}"].ToString());
                var bookingInformation = new List<object> { cachedBookingInformation };
                var model = await _pageFactory.Build(currentPage, viewModel, baseForm, guid, null, bookingInformation);

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = model
                };
            }

            if (!element.Properties.CheckYourBooking)
            {
                await ReserveAppointment(element, viewModel, baseForm.BaseURL, guid);

                _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

                return new ProcessRequestEntity
                {
                    Page = currentPage
                };
            }

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

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

        private async Task<ProcessRequestEntity> ProcessCheckYourBooking(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path, Booking element)
        {
            await ReserveAppointment(element, viewModel, baseForm.BaseURL, guid);

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

            return new ProcessRequestEntity { Page = currentPage };
        }

        private async Task<Guid> ReserveAppointment(Booking bookingElement, Dictionary<string, dynamic> viewModel, string form, string guid)
        {
            var reservedBookingId = bookingElement.ReservedIdQuestionId;
            var reservedBookingAppointmentId = bookingElement.ReservedAppointmentIdQuestionId;

            var reservedBookingDate = bookingElement.ReservedDateQuestionId;
            var reservedBookingStartTime = bookingElement.ReservedStartTimeQuestionId;
            var reservedBookingEndTime = bookingElement.ReservedEndTimeQuestionId;

            var currentlySelectedBookingDate = bookingElement.DateQuestionId;
            var currentlySelectedBookingStartTime = bookingElement.StartTimeQuestionId;
            var currentlySelectedBookingEndTime = bookingElement.EndTimeQuestionId;

            var bookingRequest = await _mappingService.MapBookingRequest(guid, bookingElement, viewModel, form);
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

            var result = await _bookingProviders.Get(bookingElement.Properties.BookingProvider)
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
            var bookingProvider = _bookingProviders.Get(bookingElement.Properties.BookingProvider);
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
}
