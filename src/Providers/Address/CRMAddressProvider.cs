namespace form_builder.Providers.Address;

public class CRMAddressProvider(IVerintServiceGateway verintServiceGateway) : IAddressProvider
{
    public string ProviderName => "CLOUDCRM";

    public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode)
    {
        var response = await verintServiceGateway.SearchForPropertyByPostcode(streetOrPostcode);

        return response ?? new List<AddressSearchResult>();
    }
}