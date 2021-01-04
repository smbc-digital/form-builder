using form_builder.Constants;
using form_builder.ContentFactory;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.Booking;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using form_builder.Utils.Extesions;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using System;
using System.Collections.Generic;
using form_builder.Exceptions;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Services.MappingService;
using form_builder.Models.Booking;
using form_builder.Services.BookingService.Entities;
using Microsoft.Extensions.Options;
using form_builder.Configuration;

namespace form_builder.Services.BookingService
{
    public interface IBookingService
    {
        Task<BookingProcessEntity> Get(
            string formName,
            Page currentPage,
            string guid);

        Task<ProcessRequestEntity> ProcessBooking(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path);

        Task ProcessMonthRequest(
            Dictionary<string, object> viewModel,
            FormSchema baseForm,
            Page currentPage,
            string guid);
    }

    public class BookingService : IBookingService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IPageHelper _pageHelper;
        private readonly IEnumerable<IBookingProvider> _bookingProviders;
        private readonly IPageFactory _pageFactory;
        private readonly IMappingService _mappingService;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

        public BookingService(
            IDistributedCacheWrapper distributedCache,
            IPageHelper pageHelper,
            IEnumerable<IBookingProvider> bookingProviders,
            IPageFactory pageFactory,
            IMappingService mappingService,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration)
        {
            _distributedCache = distributedCache;
            _pageHelper = pageHelper;
            _bookingProviders = bookingProviders;
            _pageFactory = pageFactory;
            _mappingService = mappingService;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
        }

        public async Task<BookingProcessEntity> Get(string baseUrl, Page currentPage, string guid)
        {
            var bookingElement = currentPage.Elements
                .Where(_ => _.Type == EElementType.Booking)
                .FirstOrDefault();

            var appointmentTimes = new List<AvailabilityDayResponse>();

            var bookingInformationCacheKey = $"{bookingElement.Properties.QuestionId}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";
            var cachedAnswers = _distributedCache.GetString(guid);

            if (cachedAnswers != null)
            {
                var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                if (convertedAnswers.FormData.ContainsKey(bookingInformationCacheKey))
                {
                    var cachedBookingInformation = JsonConvert.DeserializeObject<BookingInformation>(convertedAnswers.FormData[bookingInformationCacheKey].ToString());
                    var cachedInfo = new List<object> { cachedBookingInformation };
                    return new BookingProcessEntity { BookingInfo = cachedInfo };
                }
            }

            var bookingProvider = _bookingProviders.Get(bookingElement.Properties.BookingProvider);

            var nextAvailability = await RetrieveNextAvailability(bookingElement, bookingProvider);

            if(nextAvailability.BookingHasNoAvailableAppointments)
                return new BookingProcessEntity { BookingHasNoAvailableAppointments = true };

            appointmentTimes = await bookingProvider.GetAvailability(new AvailabilityRequest
            {
                StartDate = nextAvailability.DayResponse.Date,
                EndDate = nextAvailability.DayResponse.Date.LastDayOfTheMonth(),
                AppointmentId = bookingElement.Properties.AppointmentType,
                OptionalResources = bookingElement.Properties.OptionalResources
            });

            var bookingInformation = new BookingInformation
            {
                Appointments = appointmentTimes,
                CurrentSearchedMonth = new DateTime(nextAvailability.DayResponse.Date.Year, nextAvailability.DayResponse.Date.Month, 1),
                FirstAvailableMonth = new DateTime(nextAvailability.DayResponse.Date.Year, nextAvailability.DayResponse.Date.Month, 1),
                IsFullDayAppointment = nextAvailability.DayResponse.IsFullDayAppointment
            };

            if (nextAvailability.DayResponse.IsFullDayAppointment)
            {
                bookingInformation.AppointmentStartTime = DateTime.Today.Add(nextAvailability.DayResponse.AppointmentTimes.First().StartTime);
                bookingInformation.AppointmentEndTime = DateTime.Today.Add(nextAvailability.DayResponse.AppointmentTimes.First().EndTime);
            }

            _pageHelper.SaveFormData(bookingInformationCacheKey, bookingInformation, guid, baseUrl);
            var bookingInfo = new List<object> { bookingInformation };
            return new BookingProcessEntity { BookingInfo = bookingInfo };
        }

        public async Task<ProcessRequestEntity> ProcessBooking(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

            switch (subPath)
            {
                case BookingConstants.CHECK_YOUR_BOOKING:
                    return await ProcessCheckYourBooking(viewModel, currentPage, baseForm, guid, path);
                default:
                    return await ProcessDateAndTime(viewModel, currentPage, baseForm, guid, path);
            }
        }

        private async Task<BoookingNextAvailabilityEntity> RetrieveNextAvailability(IElement bookingElement, IBookingProvider bookingProvider) 
        {
            var bookingNextAvailabilityCachedKey = $"{bookingElement.Properties.BookingProvider}-{bookingElement.Properties.AppointmentType}{bookingElement.Properties.OptionalResources.CreateKeyFromResources()}";
            var OptionalResources = bookingElement.Properties.OptionalResources.Select(_ => _.Quantity);
            var bookingNextAvailabilityCachedResponse = _distributedCache.GetString(bookingNextAvailabilityCachedKey);

            var nextAvailability = new AvailabilityDayResponse();
            var result = new BoookingNextAvailabilityEntity();
            if (bookingNextAvailabilityCachedResponse != null)
            {
                return JsonConvert.DeserializeObject<BoookingNextAvailabilityEntity>(bookingNextAvailabilityCachedResponse);
            }
            else
            {
                try
                {
                    nextAvailability = await bookingProvider
                    .NextAvailability(new AvailabilityRequest
                    {
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(bookingElement.Properties.SearchPeriod),
                        AppointmentId = bookingElement.Properties.AppointmentType,
                        OptionalResources = bookingElement.Properties.OptionalResources
                    });
                }
                catch (BookingNoAvailabilityException)
                {
                    result = new BoookingNextAvailabilityEntity { BookingHasNoAvailableAppointments = true };
                    _ = _distributedCache.SetStringAsync(bookingNextAvailabilityCachedKey, JsonConvert.SerializeObject(result), _distributedCacheExpirationConfiguration.BookingNoAppointmentsAvailable);
                    return result;
                }
            }

            result = new BoookingNextAvailabilityEntity{ DayResponse = nextAvailability };
            _ = _distributedCache.SetStringAsync(bookingNextAvailabilityCachedKey, JsonConvert.SerializeObject(result), _distributedCacheExpirationConfiguration.Booking);
            return result;
        }

        private async Task<ProcessRequestEntity> ProcessDateAndTime(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            var bookingElement = (Booking) currentPage.Elements.First(_ => _.Type.Equals(EElementType.Booking));
            
            if (!currentPage.IsValid)
            {
                var cachedAnswers = _distributedCache.GetString(guid);

                var convertedAnswers = cachedAnswers == null
                    ? new FormAnswers { Pages = new List<PageAnswers>() }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                var cachedBookingInformation = JsonConvert.DeserializeObject<BookingInformation>(convertedAnswers.FormData[$"{bookingElement.Properties.QuestionId}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}"].ToString());
                var bookingInformation = new List<object> { cachedBookingInformation };
                var model = await _pageFactory.Build(currentPage, viewModel, baseForm, guid, null, bookingInformation);

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = model
                };
            }

            if (!bookingElement.Properties.CheckYourBooking)
            {
                await ReserveAppointment(bookingElement, viewModel, baseForm.BaseURL, guid);

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

        private async Task<ProcessRequestEntity> ProcessCheckYourBooking(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            var bookingElement = (Booking) currentPage.Elements.First(_ => _.Type.Equals(EElementType.Booking));
            await ReserveAppointment(bookingElement, viewModel, baseForm.BaseURL, guid);

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        private async Task<Guid> ReserveAppointment(Booking bookingElement, Dictionary<string, dynamic> viewModel, string form, string guid)
        {
            var reservedBookingId = bookingElement.ReservedIdQuestionId;
            var reservedBookingDate = bookingElement.ReservedDateQuestionId;
            var reservedBookingStartTime = bookingElement.ReservedStartTimeQuestionId;
            var reservedBookingEndTime = bookingElement.ReservedEndTimeQuestionId;
            var reservedBookingLocation = bookingElement.AppointmentLocation;            

            var currentlySelectedBookingDate = bookingElement.DateQuestionId;
            var currentlySelectedBookingStartTime = bookingElement.StartTimeQuestionId;
            var currentlySelectedBookingEndTime = bookingElement.EndTimeQuestionId;

            if (viewModel.ContainsKey(reservedBookingId) && !string.IsNullOrEmpty((string)viewModel[reservedBookingId]))
            {
                var currentSelectedDate = (string)viewModel[currentlySelectedBookingDate];
                var currentSelectedStartTime = (string)viewModel[currentlySelectedBookingStartTime];
                var currentSelectedEndTime = (string)viewModel[currentlySelectedBookingEndTime];
                var previouslyReservedAppointmentDate = (string)viewModel[reservedBookingDate];
                var previouslyReservedAppointmentStartTime = (string)viewModel[reservedBookingStartTime];
                var previouslyReservedAppointmentEndTime = (string)viewModel[reservedBookingEndTime];

                if (currentSelectedDate.Equals(previouslyReservedAppointmentDate) && currentSelectedStartTime.Equals(previouslyReservedAppointmentStartTime) && currentSelectedEndTime.Equals(previouslyReservedAppointmentEndTime))
                    return Guid.Parse(viewModel[reservedBookingId]);
            }

            viewModel.Remove(reservedBookingId);
            viewModel.Remove(reservedBookingDate);
            viewModel.Remove(reservedBookingStartTime);
            viewModel.Remove(reservedBookingEndTime);

            var bookingRequest = await _mappingService.MapBookingRequest(guid, bookingElement, viewModel, form);
            var result = await _bookingProviders.Get(bookingElement.Properties.BookingProvider)
                .Reserve(bookingRequest);

            viewModel.Add(reservedBookingDate, viewModel[currentlySelectedBookingDate]);
            viewModel.Add(reservedBookingStartTime, viewModel[currentlySelectedBookingStartTime]);
            viewModel.Add(reservedBookingEndTime, viewModel[currentlySelectedBookingEndTime]);
            viewModel.Add(reservedBookingId, result);

            var bookingProvider = _bookingProviders.Get(bookingElement.Properties.BookingProvider);
            var location = await bookingProvider.GetLocation(new LocationRequest
            {
                AppointmentId = bookingElement.Properties.AppointmentType,
                OptionalResources = null
            });

            if (string.IsNullOrEmpty(location))
            {
                var address = await _mappingService.MapAddress(guid, form);
                viewModel.Add(reservedBookingLocation, address.ToStringWithoutPlaceRef());
            }
            else
            {
                viewModel.Add(reservedBookingLocation, location);
            }

            return result;
        }

        public async Task ProcessMonthRequest(Dictionary<string, object> viewModel, FormSchema baseForm, Page currentPage, string guid)
        {
            if (!viewModel.ContainsKey(BookingConstants.BOOKING_MONTH_REQUEST))
                throw new ApplicationException("BookingService::ProcessMonthRequest, request for appointment did not contain requested month");

            var requestedMonth = DateTime.Parse(viewModel[BookingConstants.BOOKING_MONTH_REQUEST].ToString());

            var currentDate = DateTime.Now;
            var bookingElement = currentPage.Elements
                .Where(_ => _.Type == EElementType.Booking)
                .FirstOrDefault();

            if (requestedMonth.Month == currentDate.Month && requestedMonth.Year == currentDate.Year)
                requestedMonth = currentDate;

            if (requestedMonth > new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(bookingElement.Properties.SearchPeriod))
                throw new ApplicationException("BookingService::ProcessMonthRequest, Invalid request for appointment search, Start date provided is after allowed search period");

            if (requestedMonth < currentDate)
                throw new ApplicationException("BookingService::ProcessMonthRequest, Invalid request for appointment search, Start date provided is before today");

            var bookingSearchResultsKey = $"{bookingElement.Properties.QuestionId}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";
            var appointmentTimes = await _bookingProviders.Get(bookingElement.Properties.BookingProvider)
                .GetAvailability(new AvailabilityRequest
                {
                    StartDate = requestedMonth,
                    EndDate = requestedMonth.Date.LastDayOfTheMonth(),
                    AppointmentId = bookingElement.Properties.AppointmentType,
                    OptionalResources = bookingElement.Properties.OptionalResources
                });

            var bookingInformationCacheKey = $"{bookingElement.Properties.QuestionId}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";
            var cachedAnswers = _distributedCache.GetString(guid);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
            var cachedBookingInformation = JsonConvert.DeserializeObject<BookingInformation>(convertedAnswers.FormData[bookingInformationCacheKey].ToString());

            var bookingInformation = new BookingInformation
            {
                Appointments = appointmentTimes,
                CurrentSearchedMonth = requestedMonth,
                FirstAvailableMonth = cachedBookingInformation.FirstAvailableMonth,
                IsFullDayAppointment = cachedBookingInformation.IsFullDayAppointment,
                AppointmentEndTime = cachedBookingInformation.AppointmentEndTime,
                AppointmentStartTime = cachedBookingInformation.AppointmentStartTime
            };

            _pageHelper.SaveFormData(bookingSearchResultsKey, bookingInformation, guid, baseForm.BaseURL);
        }
    }
}
