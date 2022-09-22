using StockportGovUK.NetStandard.Gateways.StreetService;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.Models.Street;

namespace form_builder.Providers.Street
{
    public class ServiceStreetProvider : IStreetProvider
    {
        public string ProviderName => "CRM";

        private readonly IStreetServiceGateway _streetServiceGateway;
        public ServiceStreetProvider(IStreetServiceGateway streetServiceGateway) => _streetServiceGateway = streetServiceGateway;

        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string street)
        {
            var response = await _streetServiceGateway.SearchAsync(new StreetSearch { StreetProvider = EStreetProvider.CRM, SearchTerm = street });
            return response.ResponseContent;
        }
    }
}
