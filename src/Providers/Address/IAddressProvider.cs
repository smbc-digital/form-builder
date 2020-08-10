using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Addresses;

namespace form_builder.Providers.Address
{
    public interface IAddressProvider
    {
        string ProviderName { get; }

        Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode);
    }
}