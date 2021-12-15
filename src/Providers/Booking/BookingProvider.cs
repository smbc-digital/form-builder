using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using form_builder.Exceptions;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways.BookingService;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;

namespace form_builder.Providers.Booking
{
    public class BookingProvider : IBookingProvider
    {
        public string ProviderName { get => "SMBC"; }
        private readonly IBookingServiceGateway _gateway;

        public BookingProvider(IBookingServiceGateway gateway) => _gateway = gateway;

        public async Task<AvailabilityDayResponse> NextAvailability(AvailabilityRequest request)
        {
            var result = await _gateway.NextAvailability(request);

            if (result.StatusCode.Equals(HttpStatusCode.BadRequest))
                throw new ApplicationException($"BookingProvider::NextAvailability, BookingServiceGateway received a bad request, Request:{JsonConvert.SerializeObject(request)}, Response: {JsonConvert.SerializeObject(result)}");

            if (result.StatusCode.Equals(HttpStatusCode.NotFound))
                throw new BookingNoAvailabilityException($"BookingProvider::NextAvailability, BookingServiceGateway returned with 404 status code, no appointments available within the requested timeframe {request.StartDate} to {request.EndDate} for appointentId {request.AppointmentId}");

            if (!result.IsSuccessStatusCode)
                throw new ApplicationException($"BookingProvider::NextAvailability, BookingServiceGateway returned with non success status code of {result.StatusCode}, Response: {JsonConvert.SerializeObject(result)}");

            return result.ResponseContent;
        }

        public async Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request)
        {
            var result = await _gateway.GetAvailability(request);

            if (result.StatusCode.Equals(HttpStatusCode.BadRequest))
                throw new ApplicationException($"BookingProvider::GetAvailability, BookingServiceGateway received a bad request, Request:{JsonConvert.SerializeObject(request)}, Response: {JsonConvert.SerializeObject(result)}");

            if (result.StatusCode.Equals(HttpStatusCode.NotFound))
                throw new ApplicationException($"BookingProvider::GetAvailability, BookingServiceGateway returned 404 status code, booking with id {request.AppointmentId} cannot be found");

            if (!result.IsSuccessStatusCode)
                throw new ApplicationException($"BookingProvider::GetAvailability, BookingServiceGateway returned with non success status code of {result.StatusCode}, Response: {JsonConvert.SerializeObject(result)}");

            return result.ResponseContent;
        }

        public async Task<Guid> Reserve(BookingRequest request)
        {
            var result = await _gateway.Reserve(request);

            if (result.StatusCode.Equals(HttpStatusCode.BadRequest))
                throw new ApplicationException($"BookingProvider::Reserve, BookingServiceGateway received a bad request, Request:{JsonConvert.SerializeObject(request)}, Response: {JsonConvert.SerializeObject(result)}");

            if (result.StatusCode.Equals(HttpStatusCode.NotFound))
                throw new ApplicationException($"BookingProvider::Reserve, BookingServiceGateway returned 404 status code, booking with id {request.AppointmentId} cannot be found");

            if (!result.IsSuccessStatusCode)
                throw new ApplicationException($"BookingProvider::Reserve, BookingServiceGateway returned with non success status code of {result.StatusCode}, Response: {JsonConvert.SerializeObject(result)}");

            return result.ResponseContent;
        }

        public async Task<string> GetLocation(LocationRequest request)
        {
            var result = await _gateway.GetLocation(request);

            if (result.StatusCode.Equals(HttpStatusCode.BadRequest))
                throw new ApplicationException($"BookingProvider::GetLocation, BookingServiceGateway returned 400 status code, gateway received a bad request, Request:{JsonConvert.SerializeObject(request)}, Response: {JsonConvert.SerializeObject(result)}");

            if (result.StatusCode.Equals(HttpStatusCode.NotFound))
                throw new ApplicationException($"BookingProvider::GetLocation, BookingServiceGateway returned not found for appointmentId: {request.AppointmentId}");

            if (!result.IsSuccessStatusCode)
                throw new ApplicationException($"BookingProvider::GetLocation, BookingServiceGateway returned with non success status code of {result.StatusCode}, Response: {JsonConvert.SerializeObject(result)}");

            return result.ResponseContent;
        }

        public async Task<BookingInformationResponse> GetBooking(Guid bookingId)
        {
            var response = await _gateway.GetBooking(bookingId);

            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
                throw new ApplicationException($"BookingProvider::GetBooking, BookingServiceGateway returned not found for bookingId: {bookingId}");

            if (response.StatusCode.Equals(HttpStatusCode.BadRequest))
                throw new ApplicationException($"BookingProvider::GetBooking, BookingServiceGateway returned 400 status code, gateway received a bad request for bookingId {bookingId}, Response: {JsonConvert.SerializeObject(response)}");

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"BookingProvider::GetBooking, Gateway returned with non success status code of {response.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

            return response.ResponseContent;
        }

        public async Task Cancel(Guid bookingId)
        {
            var response = await _gateway.Cancel(bookingId.ToString());

            if (response.StatusCode.Equals(HttpStatusCode.BadRequest))
                throw new ApplicationException($"BookingProvider::Cancel, BookingServiceGateway returned 400 status code, Gateway received bad request, Request:{bookingId}, Response: {JsonConvert.SerializeObject(response)}");

            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
                throw new ApplicationException($"BookingProvider::Cancel, BookingServiceGateway return 404 not found for bookingId {bookingId}");

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"BookingProvider::Cancel, BookingServiceGateway returned with non success status code of {response.StatusCode}, Response: {JsonConvert.SerializeObject(response)}");
        }

        public async Task Confirm(ConfirmationRequest request)
        {
            var result = await _gateway.Confirmation(request);

            if (result.StatusCode.Equals(HttpStatusCode.BadRequest))
                throw new ApplicationException($"BookingProvider::Confirmation, BookingServiceGateway received a bad request, Request:{JsonConvert.SerializeObject(request)}, Response: {JsonConvert.SerializeObject(result)}");

            if (result.StatusCode.Equals(HttpStatusCode.NotFound))
                throw new ApplicationException($"BookingProvider::Confirmation, BookingServiceGateway returned 404 status code, booking with id {request.BookingId} cannot be found");

            if (!result.IsSuccessStatusCode)
                throw new ApplicationException($"BookingProvider::Confirmation, BookingServiceGateway returned with non success status code of {result.StatusCode}, Response: {JsonConvert.SerializeObject(result)}");
        }
    }
}
