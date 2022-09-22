using StockportGovUK.NetStandard.Gateways.Models.Booking.Response;

namespace form_builder.Services.BookingService.Entities
{
    public class BookingNextAvailabilityEntity
    {
        public AvailabilityDayResponse DayResponse { get; set; }
        public bool BookingHasNoAvailableAppointments { get; set; }
    }
}
