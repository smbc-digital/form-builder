using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Addresses;

namespace form_builder.Providers.Street
{
    public interface IStreetProvider
    {
        string ProviderName { get; }

        Task<IEnumerable<AddressSearchResult>> SearchAsync(string street);
    }
}