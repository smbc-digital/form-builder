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

namespace form_builder.Services.BookingService
{
    public interface IBookingService
    {
        Task<List<object>> Get(
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

        public BookingService(
            IDistributedCacheWrapper distributedCache,
            IPageHelper pageHelper,
            IEnumerable<IBookingProvider> bookingProviders,
            IPageFactory pageFactory,
            IMappingService mappingService)
        {
            _distributedCache = distributedCache;
            _pageHelper = pageHelper;
            _bookingProviders = bookingProviders;
            _pageFactory = pageFactory;
            _mappingService = mappingService;
        }

        public async Task<List<object>> Get(
            string baseUrl,
            Page currentPage,
            string guid)
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
                    return new List<object> { cachedBookingInformation };
                }
            }

            var bookingProvider = _bookingProviders.Get(bookingElement.Properties.BookingProvider);

            var nextAvailability = new AvailabilityDayResponse();
            try
            {
                nextAvailability = await bookingProvider
                .NextAvailability(new AvailabilityRequest
                {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(bookingElement.Properties.SearchPeriod),
                    AppointmentId = bookingElement.Properties.AppointmentType
                });
            }
            catch (BookingNoAvailabilityException)
            {
                // Booking Provider threw NoAvailabilityException 
                // Appointment has no availabity within timeframe
                // Navigate to the no appointments page.
            }

            appointmentTimes = await bookingProvider.GetAvailability(new AvailabilityRequest
            {
                StartDate = nextAvailability.Date,
                EndDate = nextAvailability.Date.LastDayOfTheMonth(),
                AppointmentId = bookingElement.Properties.AppointmentType
            });

            var bookingInformation = new BookingInformation
            {
                Appointments = appointmentTimes,
                CurrentSearchedMonth = nextAvailability.Date,
                FirstAvailableMonth = nextAvailability.Date,
                IsFullDayAppointment = nextAvailability.IsFullDayAppointment
            };

            if (nextAvailability.IsFullDayAppointment)
            {
                bookingInformation.AppointmentStartTime = DateTime.Today.Add(nextAvailability.AppointmentTimes.First().StartTime);
                bookingInformation.AppointmentEndTime = DateTime.Today.Add(nextAvailability.AppointmentTimes.First().EndTime);
            }

            _pageHelper.SaveFormData(bookingInformationCacheKey, bookingInformation, guid, baseUrl);

            return new List<object> { bookingInformation };
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

        private async Task<ProcessRequestEntity> ProcessDateAndTime(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            var bookingElement = currentPage.Elements.First(_ => _.Type.Equals(EElementType.Booking));

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
            var bookingElement = currentPage.Elements.First(_ => _.Type.Equals(EElementType.Booking));
            await ReserveAppointment(bookingElement, viewModel, baseForm.BaseURL, guid);

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        private async Task<Guid> ReserveAppointment(IElement bookingElement, Dictionary<string, dynamic> viewModel, string form, string guid)
        {
            var reservedBookingId = $"{bookingElement.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_ID}";
            var reservedBookingDate = $"{bookingElement.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_DATE}";
            var reservedBookingTime = $"{bookingElement.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_TIME}";
            var currentlySelectedBookingDate = $"{bookingElement.Properties.QuestionId}{BookingConstants.APPOINTMENT_DATE}";
            var currentlySelectedBookingTime = $"{bookingElement.Properties.QuestionId}{BookingConstants.APPOINTMENT_TIME}";

            if (viewModel.ContainsKey(reservedBookingId) && !string.IsNullOrEmpty((string)viewModel[reservedBookingId]))
            {
                var currentSelectedDate = (string)viewModel[currentlySelectedBookingDate];
                var currentSelectedTime = (string)viewModel[currentlySelectedBookingTime];
                var previouslyReservedAppointmentDate = (string)viewModel[reservedBookingDate];
                var previouslyReservedAppointmentTime = (string)viewModel[reservedBookingTime];

                if (currentSelectedDate.Equals(previouslyReservedAppointmentDate) && currentSelectedTime.Equals(previouslyReservedAppointmentTime))
                    return Guid.Parse(viewModel[reservedBookingId]);
            }

            viewModel.Remove(reservedBookingId);
            viewModel.Remove(reservedBookingDate);
            viewModel.Remove(reservedBookingTime);

            var bookingRequest = await _mappingService.MapBookingRequest(guid, bookingElement, viewModel, form);
            var result = await _bookingProviders.Get(bookingElement.Properties.BookingProvider).Reserve(bookingRequest);

            viewModel.Add(reservedBookingDate, viewModel[currentlySelectedBookingDate]);
            viewModel.Add(reservedBookingTime, viewModel[currentlySelectedBookingTime]);
            viewModel.Add(reservedBookingId, result);

            return result;
        }
        
        public async Task ProcessMonthRequest(Dictionary<string, object> viewModel, FormSchema baseForm, Page currentPage, string guid)
        {
            if(!viewModel.ContainsKey(BookingConstants.BOOKING_MONTH_REQUEST))
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
                    AppointmentId = bookingElement.Properties.AppointmentType
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
