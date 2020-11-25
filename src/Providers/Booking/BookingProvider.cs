using Microsoft.AspNetCore.Mvc;
using StockportGovUK.NetStandard.Gateways.BookingService;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            return result.ResponseContent;
        }

        public async Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request)
        {
            var result = await _gateway.GetAvailability(request);
            return result.ResponseContent;
        }

        public async Task<Guid> Reserve(BookingRequest request)
        {
            var result = await _gateway.Reserve(request);
            return result.ResponseContent;
        }

        public async Task<IActionResult> HasAvailability(AvailabilityRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> Confirm(ConfirmationRequest request)
        {
            throw new NotImplementedException();
        }
        public async Task<IActionResult> Cancel(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
