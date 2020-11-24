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
using System.Globalization;
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
            var bookingElement = currentPage.Elements
                .Where(_ => _.Type == EElementType.Booking)
                .FirstOrDefault();

            var appointmentTimes = new List<AvailabilityDayResponse>();

            var bookingSearchResultsKey = $"{bookingElement.Properties.QuestionId}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";
            var cachedAnswers = _distributedCache.GetString(guid);

            if(cachedAnswers != null)
            {
                var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                if (convertedAnswers.FormData.ContainsKey(bookingSearchResultsKey))
                    return JsonConvert.DeserializeObject<List<AvailabilityDayResponse>>(convertedAnswers.FormData[bookingSearchResultsKey].ToString());
            }

            var bookingProvider = _bookingProviders.Get(bookingElement.Properties.BookingProvider);

            var nextAvailability = await bookingProvider
                .NextAvailability(new AvailabilityRequest{  
                    StartDate = DateTime.Now, 
                    EndDate = DateTime.Now.AddMonths(bookingElement.Properties.SearchPeriod),
                    AppointmentId = bookingElement.Properties.AppointmentType
                });

            // Is no next appointment within allowed timeframe
            if (nextAvailability.Date < DateTime.Now)
            {
                _pageHelper.SaveFormData(bookingSearchResultsKey, appointmentTimes, guid);
                return appointmentTimes;
            }

            appointmentTimes = await bookingProvider.GetAvailability(new AvailabilityRequest {
                    StartDate = nextAvailability.Date,
                    EndDate = nextAvailability.Date.LastDayOfTheMonth(),
                    AppointmentId = bookingElement.Properties.AppointmentType
              });

            _pageHelper.SaveFormData(bookingSearchResultsKey, appointmentTimes, guid);

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

            // If page is valid, i.e. they have selected a Date then we can simply return the current page
            // If page is not valid
            // We need to repopulate the calendar from cached answers
            var bookingElement = currentPage.Elements.First(_ => _.Type.Equals(EElementType.Booking));

            if(!currentPage.IsValid)
            {
                var cachedAnswers = _distributedCache.GetString(guid);

                var convertedAnswers = cachedAnswers == null
                    ? new FormAnswers { Pages = new List<PageAnswers>() }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                var bookingSearchResults = (convertedAnswers.FormData[$"{bookingElement.Properties.QuestionId}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}"] as IEnumerable<object>).ToList();
                var model = await _pageFactory.Build(currentPage, viewModel, baseForm, guid, null, bookingSearchResults);

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
            //Store appointmenID
        }
    }
}
