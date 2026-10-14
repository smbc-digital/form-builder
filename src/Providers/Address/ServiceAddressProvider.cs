namespace form_builder.Providers.Address;

public class ServiceAddressProvider(IAddressServiceGateway addressServiceGateway)
    : IAddressProvider
{
    public string ProviderName => "CRM";

    public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode)
    {
        var response = await addressServiceGateway.SearchAsync(new AddressSearch { AddressProvider = EAddressProvider.CRM, SearchTerm = streetOrPostcode });

        return response.ResponseContent;
    }
}