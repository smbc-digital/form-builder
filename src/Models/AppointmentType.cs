using StockportGovUK.NetStandard.Gateways.Models.Booking.Request;

namespace form_builder.Models
{
    public class AppointmentType
    {
        public string Environment { get; set; }
        public Guid AppointmentId { get; set; }
        public string AppointmentIdKey { get; set; }
        public List<BookingResource> OptionalResources { get; set; } = new();
        public bool NeedsAppointmentIdMapping => AppointmentId.Equals(Guid.Empty) && !string.IsNullOrEmpty(AppointmentIdKey);
    }
}