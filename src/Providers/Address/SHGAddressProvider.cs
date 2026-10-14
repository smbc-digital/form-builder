namespace form_builder.Providers.Address;

public class SHGAddressProvider(IGateway gateway, IOptions<SHGAddressProviderConfiguration> shgAddressProviderConfig) : IAddressProvider
{
    public string ProviderName => "SHG";
    private readonly SHGAddressProviderConfiguration _shgAddressProviderConfiguration = shgAddressProviderConfig.Value;

    public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode)
    {
        gateway.ChangeAuthenticationHeader(shgAddressProviderConfig.Value.Key);
        var response = await gateway.GetAsync($"{_shgAddressProviderConfiguration.Host}/api/v1/DomesticInternal/get-addresses?postcode={streetOrPostcode}");
        var result = await response.Content.ReadAsStringAsync();
        var addresses = JsonConvert.DeserializeObject<List<StockportGovUK.NetStandard.Gateways.Models.Verint.Address>>(result); 

        return addresses.Select(address => new AddressSearchResult 
        {
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            AddressLine3 = address.AddressLine3,
            Postcode = address.Postcode,
            UniqueId = address.UPRN
        });
    }
}