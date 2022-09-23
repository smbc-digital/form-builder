using StockportGovUK.NetStandard.Gateways.Models.Booking.Request;

namespace form_builder.Extensions
{
    public static class BookingResourceExtensions
    {
        public static string CreateKeyFromResources(this IEnumerable<BookingResource> bookingResources)
        {
            if (!bookingResources.Any())
                return string.Empty;

            return bookingResources.Aggregate(string.Empty, (acc, cur) => $"{acc}-{cur.Quantity}{cur.ResourceId.ToString()}");
        }
    }
}
