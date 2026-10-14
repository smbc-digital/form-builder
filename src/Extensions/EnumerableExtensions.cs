namespace form_builder.Extensions;

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

    public static IFormatter Get(this IEnumerable<IFormatter> value, string formatterName) =>
        value.Single(_ => _.FormatterName.Equals(formatterName));

    public static ISubmitProvider Get(this IEnumerable<ISubmitProvider> value, string providerName) =>
        value.Single(_ => _.ProviderName.Equals(providerName));

    public static ITemplatedEmailProvider Get(this IEnumerable<ITemplatedEmailProvider> value, string providerName) =>
        value.Single(_ => _.ProviderName.Equals(providerName));

    public static IEnabledForProvider Get(this IEnumerable<IEnabledForProvider> value, EEnabledFor providerName) =>
        value.Single(_ => _.Type.Equals(providerName));

    public static IFileStorageProvider Get(this IEnumerable<IFileStorageProvider> value, string providerName) =>
        value.Single(_ => _.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

    public static IAnalyticsProvider Get(this IEnumerable<IAnalyticsProvider> value, string providerName) =>
        value.Single(_ => _.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
}