using StockportGovUK.NetStandard.Gateways.Models.Addresses;
using StockportGovUK.NetStandard.Gateways.VerintService;

namespace form_builder.Providers.Address;

public class CRMAddressProvider(IVerintServiceGateway verintServiceGateway) : IAddressProvider
{
    public string ProviderName => "CLOUDCRM";

    private readonly IVerintServiceGateway _verintServiceGateway = verintServiceGateway;

    public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode)
    {
        var response = await _verintServiceGateway.SearchForPropertyByPostcode(streetOrPostcode);

        return response ?? new List<AddressSearchResult>();
    }
}