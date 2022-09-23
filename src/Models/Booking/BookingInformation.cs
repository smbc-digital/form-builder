using StockportGovUK.NetStandard.Gateways.Models.Booking.Response;

namespace form_builder.Models.Booking
{
    public class BookingInformation
    {
        public Guid AppointmentTypeId { get; set; }
        public DateTime CurrentSearchedMonth { get; set; }
        public DateTime FirstAvailableMonth { get; set; }
        public List<AvailabilityDayResponse> Appointments { get; set; }
        public bool IsFullDayAppointment { get; set; }
        public DateTime AppointmentStartTime { get; set; }
        public DateTime AppointmentEndTime { get; set; }
    }
}