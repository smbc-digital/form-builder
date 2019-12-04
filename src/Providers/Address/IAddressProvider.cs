using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.Address
{
    public interface IAddressProvider
    {

        string ProviderName { get; }

        Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode);

        //AddressSearchResult Search(string postcode);

        //AddressDetails GetAddressDetails(string id);
    }
}