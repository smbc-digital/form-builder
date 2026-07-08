using StockportGovUK.NetStandard.Gateways.AddressService;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;

namespace form_builder.Providers.Address;

public class ServiceAddressProvider(IAddressServiceGateway addressServiceGateway, ILogger<IAddressProvider> logger)
    : IAddressProvider
{
    public string ProviderName => "CRM";

    private readonly IAddressServiceGateway _addressServiceGateway = addressServiceGateway;
    private readonly ILogger<IAddressProvider> _logger = logger;

    public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode)
    {
        var response = await _addressServiceGateway.SearchAsync(new AddressSearch { AddressProvider = EAddressProvider.CRM, SearchTerm = streetOrPostcode });

        return response.ResponseContent;
    }
}