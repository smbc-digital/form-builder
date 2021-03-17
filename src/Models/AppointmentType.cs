using System;
using System.Collections.Generic;
using StockportGovUK.NetStandard.Models.Booking.Request;

namespace form_builder.Models
{
    public class AppointmentType
    {
        public string Environment { get; set; }
        public Guid AppointmentId { get; set; }
        public List<BookingResource> OptionalResources { get; set; } = new List<BookingResource>();
    }
}