using System;

namespace form_builder.Services.BookingService.Entities
{
    public class CancelledAppointmentInformation
    {
        public string FormName { get; set; }
        public string BaseURL { get; set; }
        public string StartPageUrl { get; set; }
        public Guid Id { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool Cancellable { get; set; }
        public string Hash { get; set; }
        public bool IsFullday { get; set; }
    }
}
