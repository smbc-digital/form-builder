using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.Booking
{
    public interface IBookingProvider
    {
        string ProviderName { get; }

        Task<AvailabilityDayResponse> NextAvailability(AvailabilityRequest request);
        Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request);
        Task<Guid> Reserve(BookingRequest request);
    }
}
