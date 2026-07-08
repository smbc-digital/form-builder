using StockportGovUK.NetStandard.Gateways.Models.Addresses;
using StockportGovUK.NetStandard.Gateways.VerintService;

namespace form_builder.Providers.Street;

public class CRMStreetProvider(IVerintServiceGateway verintServiceGateway) : IStreetProvider
{
    public string ProviderName => "CRMStreet";

    private readonly IVerintServiceGateway _verintServiceGateway = verintServiceGateway;

    public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string street)
    {
        var response = await _verintServiceGateway.GetStreetByReference(street);
        return response;
    }
}