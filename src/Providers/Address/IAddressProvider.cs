using StockportGovUK.NetStandard.Gateways.Models.Addresses;

namespace form_builder.Providers.Address
{
    public interface IAddressProvider
    {
        string ProviderName { get; }

        Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode);
    }
}