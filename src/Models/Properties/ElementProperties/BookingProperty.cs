using System.Collections.Generic;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string BookingProvider { get; set; }
        public int SearchPeriod = 12;
        public List<AppointmentType> AppointmentTypes { get; set; }  = new List<AppointmentType>();
        public bool CheckYourBooking { get; set; }
        public string AppointmentTime { get; set; }
        public string NextAvailableIAG { get; set; } = "This is the next available appointment.";
        public string NoAvailableTimeForBookingType { get; set; } = "appointments";
        public string CustomerAddressId { get; set; }
    }
}