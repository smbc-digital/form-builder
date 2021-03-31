using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;

namespace form_builder.Providers.Booking
{
    public interface IBookingProvider
    {
        string ProviderName { get; }

        Task<AvailabilityDayResponse> NextAvailability(AvailabilityRequest request);
        Task<List<AvailabilityDayResponse>> GetAvailability(AvailabilityRequest request);
        Task<Guid> Reserve(BookingRequest request);
        Task<string> GetLocation(LocationRequest request);
        Task<BookingInformationResponse> GetBooking(Guid bookingId);
        Task Cancel(Guid bookingId);
    }
}
