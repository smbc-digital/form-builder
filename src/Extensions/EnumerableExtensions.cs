using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Providers.Address;

namespace form_builder.Extensions
{
    public static class EnumerableExtensions
    {
        public static IAddressProvider Get(this IEnumerable<IAddressProvider> value, string providerName)
        {
            return value.Single(_ => _.ProviderName == providerName);
        }
    }
}
