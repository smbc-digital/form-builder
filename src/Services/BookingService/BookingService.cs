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
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Services.BookingService
{
    public interface IBookingService
    {
        Task<List<AvailabilityDayResponse>> Get(
            Page currentPage,
            string guid);

        Task<ProcessRequestEntity> ProcessBooking(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path);
    }

    public class BookingService : IBookingService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IPageHelper _pageHelper;
        private readonly IEnumerable<IBookingProvider> _bookingProviders;
        private readonly IPageFactory _pageFactory;
        public BookingService(
            IDistributedCacheWrapper distributedCache,
            IPageHelper pageHelper,
            IEnumerable<IBookingProvider> bookingProviders,
            IPageFactory pageFactory)
        {
            _distributedCache = distributedCache;
            _pageHelper = pageHelper;
            _bookingProviders = bookingProviders;
            _pageFactory = pageFactory;
        }

        public async Task<List<AvailabilityDayResponse>> Get(
            Page currentPage,
            string guid)
        {
            var bookingElement = currentPage.Elements.Where(_ => _.Type == EElementType.Booking)
                .FirstOrDefault();

            var searchResults = _distributedCache.GetString($"{bookingElement.Properties.AppointmentType}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}");

            // Cached response stored 
            if(!string.IsNullOrEmpty(searchResults))
                return JsonConvert.DeserializeObject<List<AvailabilityDayResponse>>(searchResults);
            
            var bookingProvider = _bookingProviders.Get(bookingElement.Properties.BookingProvider);

            var nextAvailability = await bookingProvider
                .NextAvailability(new AvailabilityRequest{  
                    StartDate = DateTime.Now, 
                    EndDate = DateTime.Now.AddMonths(bookingElement.Properties.SearchPeriod),
                    AppointmentId = bookingElement.Properties.AppointmentType
                });

            // Is this next appointment within allowed timeframe
                // If not ->

            var daysInMonth = DateTime.DaysInMonth(nextAvailability.Date.Year, nextAvailability.Date.Month);
            var endDateSearch = new DateTime(nextAvailability.Date.Year, nextAvailability.Date.Month, daysInMonth, 23, 59, 59);

            var appointmentTimes = await bookingProvider.GetAvailability(new AvailabilityRequest {
                    StartDate = nextAvailability.Date,
                    EndDate = endDateSearch,
                    AppointmentId = bookingElement.Properties.AppointmentType
              });

            //Save appointmentTimes, need to append search month also to this + how long saved in cache.
            _distributedCache.SetStringAsync($"{bookingElement.Properties.AppointmentType}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}", JsonConvert.SerializeObject(appointmentTimes));

            // Return View
            return appointmentTimes;
        }

        public async Task<ProcessRequestEntity> ProcessBooking(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string guid, string path)
        {
            viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

            // Handle check your booking via subpath not sure if i like this approach, might change
            // to just a seperate page with an action on POST to call an endpoint, this will give
            // move customisation but would require adding the endpoint within FormActions of the json.
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

            // Process post request

            // If page is valid, i.e. they have selected a Date then we can siply return the current page
            // If page is not valid
            // We need to repopulate the calendar from cached answers
            var bookingElement = currentPage.Elements.First(_ => _.Type.Equals(EElementType.Booking));

            if(!currentPage.IsValid)
            {
                var cachedResults = _distributedCache.GetString($"{bookingElement.Properties.AppointmentType}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}");

                var convertedAnswers = JsonConvert.DeserializeObject<IEnumerable<object>>(cachedResults);
                var model = await _pageFactory.Build(currentPage, viewModel, baseForm, guid, null, convertedAnswers.ToList());

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = model
                };
            }

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

            //Return page if Valid.
            if(!bookingElement.Properties.CheckYourBooking){
                return new ProcessRequestEntity
                {
                    Page = currentPage
                };
            }

            // How to handle if user goes back and forward in browser, 
            // we do not want to resever the appointment again
            await ReserveAppointment(bookingElement);

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
            // Handle check your booking
            var bookingElement = currentPage.Elements.First(_ => _.Type.Equals(EElementType.Booking));
            await ReserveAppointment(bookingElement);

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        private async Task ReserveAppointment(IElement bookingElement)
        {
            //Reserve appointment
            //Needs
            //User Data
            //Selected DateTime.
            var appointmentId = _bookingProviders.Get(bookingElement.Properties.BookingProvider).Reserve(new BookingRequest());
        }
    }
}
