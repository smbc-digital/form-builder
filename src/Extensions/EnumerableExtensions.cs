using System.Collections.Generic;
using System.Linq;
using form_builder.Providers.Address;
using form_builder.Providers.Organisation;

namespace form_builder.Extensions
{
    public static class EnumerableExtensions
    {
        public static IAddressProvider Get(this IEnumerable<IAddressProvider> value, string providerName)
        {
            return value.Single(_ => _.ProviderName == providerName);
        }

        public static IOrganisationProvider Get(this IEnumerable<IOrganisationProvider> value, string providerName)
        {
            return value.Single(_ => _.ProviderName == providerName);
        }
    }
}
