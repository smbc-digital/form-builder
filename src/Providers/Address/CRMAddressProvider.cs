using StockportGovUK.NetStandard.Gateways.VerintService;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;

namespace form_builder.Providers.Address
{
    public class CRMAddressProvider : IAddressProvider
    {
        public string ProviderName => "CRM";

        private readonly IVerintServiceGateway _verintServiceGateway;

        public CRMAddressProvider(IVerintServiceGateway verintServiceGateway) => _verintServiceGateway = verintServiceGateway;

        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode)
        {
            var response = await _verintServiceGateway.SearchForPropertyByPostcode(streetOrPostcode);

            return response.ResponseContent ?? new List<AddressSearchResult>();
        }
    }
}
