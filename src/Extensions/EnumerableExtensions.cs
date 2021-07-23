using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Enum;
using form_builder.Providers.Address;
using form_builder.Providers.Booking;
using form_builder.Providers.EnabledFor;
using form_builder.Providers.FileStorage;
using form_builder.Providers.Lookup;
using form_builder.Providers.Organisation;
using form_builder.Providers.Street;
using form_builder.Providers.Submit;
using form_builder.Providers.TemplatedEmailProvider;
using form_builder.TagParsers.Formatters;

namespace form_builder.Extensions
{
    public static class EnumerableExtensions
    {
        public static ILookupProvider Get(this IEnumerable<ILookupProvider> value, string providerName) =>
            value.Single(_ => _.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        public static IAddressProvider Get(this IEnumerable<IAddressProvider> value, string providerName) =>
            value.Single(_ => _.ProviderName.Equals(providerName));

        public static IBookingProvider Get(this IEnumerable<IBookingProvider> value, string providerName) =>
            value.Single(_ => _.ProviderName.Equals(providerName));

        public static IOrganisationProvider Get(this IEnumerable<IOrganisationProvider> value, string providerName) =>
            value.Single(_ => _.ProviderName.Equals(providerName));

        public static IStreetProvider Get(this IEnumerable<IStreetProvider> value, string providerName) =>
            value.Single(_ => _.ProviderName.Equals(providerName));

        public static IFormatter Get(this IEnumerable<IFormatter> value, string formatterrName) =>
            value.Single(_ => _.FormatterName.Equals(formatterrName));

        public static ISubmitProvider Get(this IEnumerable<ISubmitProvider> value, string providerName) =>
            value.Single(_ => _.ProviderName.Equals(providerName));

        public static ITemplatedEmailProvider Get(this IEnumerable<ITemplatedEmailProvider> value, string providerName) =>
           value.Single(_ => _.ProviderName.Equals(providerName));

        public static IEnabledForProvider Get(this IEnumerable<IEnabledForProvider> value, EEnabledFor providerName) =>
           value.Single(_ => _.Type.Equals(providerName));

        public static IFileStorageProvider Get(this IEnumerable<IFileStorageProvider> value, string providerName) =>
            value.Single(_ => _.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
    }
}
