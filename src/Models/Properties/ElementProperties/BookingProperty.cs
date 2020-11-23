using System;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string BookingProvider { get; set; }
        public int SearchPeriod = 12;
        public Guid AppointmentType { get; set; }
        public bool CheckYourBooking { get; set; }
    }
}