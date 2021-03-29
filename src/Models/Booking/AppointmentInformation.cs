using System;

namespace form_builder.Models.Booking
{
    public class AppointmentInformation
    {
        public Guid AppointmentId { get; set; }
        public bool Cancellable { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsFullday { get; set; }
    }
}
