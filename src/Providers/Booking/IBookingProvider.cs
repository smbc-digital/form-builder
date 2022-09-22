using StockportGovUK.NetStandard.Gateways.Models.Booking.Request;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Response;

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
        Task Confirm(ConfirmationRequest request);
    }
}
