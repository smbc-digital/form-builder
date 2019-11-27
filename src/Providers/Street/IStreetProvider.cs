using StockportGovUK.NetStandard.Models.Addresses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.Street
{
    public interface IStreetProvider
    {
        string ProviderName { get; }

        Task<IEnumerable<AddressSearchResult>> SearchAsync(string street);
    }
}