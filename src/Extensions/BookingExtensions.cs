using StockportGovUK.NetStandard.Gateways.Models.Booking;

namespace form_builder.Extensions
{
    public static class BookingExtensions
    {
        public static bool IsEmpty(this Booking value) => value.Id.Equals(Guid.Empty)
            && value.Date.Equals(DateTime.MinValue)
            && value.StartTime.Equals(DateTime.MinValue);
    }
}
