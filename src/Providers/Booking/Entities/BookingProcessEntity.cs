using System.Collections.Generic;

namespace form_builder.Providers.Booking.Entities
{
    public class BookingProcessEntity
    {
        public List<object> BookingInfo { get; set; }
        public bool BookingHasNoAvailableAppointments { get; set; }
    }
}
