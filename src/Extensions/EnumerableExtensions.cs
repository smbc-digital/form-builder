using System.Collections.Generic;
using System.Linq;
using form_builder.Providers.Address;
using form_builder.Providers.Booking;
using form_builder.Providers.Organisation;
using form_builder.Providers.Street;
using form_builder.TagParser;

namespace form_builder.Extensions
{
    public static class EnumerableExtensions
    {
        public static IAddressProvider Get(this IEnumerable<IAddressProvider> value, string providerName) => 
            value.Single(_ => _.ProviderName == providerName);

        public static IBookingProvider Get(this IEnumerable<IBookingProvider> value, string providerName) =>
            value.Single(_ => _.ProviderName == providerName);

        public static IOrganisationProvider Get(this IEnumerable<IOrganisationProvider> value, string providerName) =>
            value.Single(_ => _.ProviderName == providerName);

        public static IStreetProvider Get(this IEnumerable<IStreetProvider> value, string providerName) =>
            value.Single(_ => _.ProviderName == providerName);

        public static IFormatter Get(this IEnumerable<IFormatter> value, string formatterrName) =>
            value.Single(_ => _.FormatterrName == formatterrName);
    }
}
