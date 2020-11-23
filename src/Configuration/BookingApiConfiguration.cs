using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Configuration
{
    public class BookingApiConfiguration
    {
        public string BaseAddress { get; set; }
        public string GetAvailability { get; set; }
        public string HasAvailability { get; set; }
        public string NextAvailability { get; set; }
        public string Cancellation { get; set; }
        public string Confirmation { get; set; }
        public string Reservation { get; set; }
    }
}
