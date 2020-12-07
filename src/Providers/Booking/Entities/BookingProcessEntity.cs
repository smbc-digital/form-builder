using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Providers.Booking.Entities
{
    public class BookingProcessEntity
    {
        public List<object> BookingInfo { get; set; }

        public bool IsBookingInfoEmpty => BookingInfo == null;
    }
}
