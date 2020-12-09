using StockportGovUK.NetStandard.Models.Booking.Response;

namespace form_builder.Services.BookingService.Entities
{
    public class BoookingNextAvailabilityEntity
    {
        public AvailabilityDayResponse DayResponse { get; set; }
        public bool BookingHasNoAvailableAppointments { get; set; }
    }
}
