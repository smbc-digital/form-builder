using StockportGovUK.NetStandard.Gateways.BookingService;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Exceptions;
using System.Net;

namespace form_builder.Providers.Booking
{
    public class BookingProvider : IBookingProvider
    {
        public string ProviderName { get => "SMBC"; }
        private readonly IBookingServiceGateway _gateway;

        public BookingProvider(IBookingServiceGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<AvailabilityDayResponse> NextAvailability(AvailabilityRequest request)
        {
            var result = await _gateway.NextAvailability(request);

            if (result.StatusCode.Equals(HttpStatusCode.BadRequest))
                throw new ApplicationException($"BookingProvider::NextAvailability, BookingServiceGateway received a bad request, Request:{Newtonsoft.Json.JsonConvert.SerializeObject(request)}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");

            if (result.StatusCode.Equals(HttpStatusCode.NotFound))
                throw new BookingNoAvailabilityException($"BookingProvider::NextAvailability, BookingServiceGateway returned with 404 status code, no appointments available within the requested timeframe {request.StartDate} to {request.EndDate} for appointentId {request.AppointmentId}");

            if(!result.IsSuccessStatusCode)
                throw new ApplicationException($"BookingProvider::NextAvailability, BookingServiceGateway returned with non success status code of {result.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");

            return result.ResponseContent;
        }

        public async Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request)
        {
            var result = await _gateway.GetAvailability(request);

            if (result.StatusCode.Equals(HttpStatusCode.BadRequest))
                throw new ApplicationException($"BookingProvider::GetAvailability, BookingServiceGateway received a bad request, Request:{Newtonsoft.Json.JsonConvert.SerializeObject(request)}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");

            if (result.StatusCode.Equals(HttpStatusCode.NotFound))
                throw new ApplicationException($"BookingProvider::GetAvailability, BookingServiceGateway returned 404 status code, booking with id {request.AppointmentId} cannot be found");

            if (!result.IsSuccessStatusCode)
                throw new ApplicationException($"BookingProvider::GetAvailability, BookingServiceGateway returned with non success status code of {result.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");

            return result.ResponseContent;
        }

        public async Task<Guid> Reserve(BookingRequest request)
        {
            var result = await _gateway.Reserve(request);

            if (result.StatusCode.Equals(HttpStatusCode.BadRequest))
                throw new ApplicationException($"BookingProvider::Reserve, BookingServiceGateway received a bad request, Request:{Newtonsoft.Json.JsonConvert.SerializeObject(request)}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");

            if (result.StatusCode.Equals(HttpStatusCode.NotFound))
                throw new ApplicationException($"BookingProvider::Reserve, BookingServiceGateway returned 404 status code, booking with id {request.AppointmentId} cannot be found");

            if (!result.IsSuccessStatusCode)
                throw new ApplicationException($"BookingProvider::Reserve, BookingServiceGateway returned with non success status code of {result.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");

            return result.ResponseContent;
        }

        // GetLocation
    }
}
