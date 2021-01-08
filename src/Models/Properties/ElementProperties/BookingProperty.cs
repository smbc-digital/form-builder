using System;
using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Booking.Request;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string BookingProvider { get; set; }
        public int SearchPeriod = 12;
        public Guid AppointmentType { get; set; }
        public bool CheckYourBooking { get; set; }
        public string AppointmentTime { get; set; }
        public string NextAvailableIAG { get; set; } = "This is the next available appointment.";
        public List<BookingResource> OptionalResources { get;set; } =  new List<BookingResource>();
        public string NoAvailableTimeForBookingType { get; set; } = "appointment";
    }
}