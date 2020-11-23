using form_builder.Configuration;
using form_builder.ContentFactory;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.Booking;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using Microsoft.Extensions.Options;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace form_builder.Services.BookingService
{
    public interface IBookingService
    {
        Task<List<AvailabilityDayResponse>> Get(
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
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path)
        {
            var bookingElement = currentPage.Elements.Where(_ => _.Type == EElementType.Booking)
                .FirstOrDefault();
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


            // Return View
            return appointmentTimes;
        }

    }
}
