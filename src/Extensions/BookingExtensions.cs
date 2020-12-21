using StockportGovUK.NetStandard.Models.Booking;
using System;

namespace form_builder.Extensions
{
    public static class BookingExtensions
    {
        public static bool IsEmpty(this Booking value) => value.Id == Guid.Empty 
            && value.Date == DateTime.MinValue 
            && value.StartTime == DateTime.MinValue;
    }
}
