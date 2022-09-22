using StockportGovUK.NetStandard.Gateways.VerintService;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;

namespace form_builder.Providers.Street
{
    public class CRMStreetProvider : IStreetProvider
    {
        public string ProviderName => "CRMStreet";

        private readonly IVerintServiceGateway _verintServiceGateway;
        public CRMStreetProvider(IVerintServiceGateway verintServiceGateway) => _verintServiceGateway = verintServiceGateway;

        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string street)
        {
            var response = await _verintServiceGateway.GetStreetByReference(street);
            return response.ResponseContent;
        }
    }
}
