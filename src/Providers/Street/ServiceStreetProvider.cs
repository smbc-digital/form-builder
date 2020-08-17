using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Providers.Street;
using StockportGovUK.NetStandard.Gateways.StreetServiceGateway;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Street;

namespace form_builder.Providers.Address
{
    public class ServiceStreetProvider : IStreetProvider
    {
        public string ProviderName => "CRM";

        private readonly IStreetServiceGateway _streetServiceGateway;

        public ServiceStreetProvider(IStreetServiceGateway streetServiceGateway)
        {
            _streetServiceGateway = streetServiceGateway;
        }

        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string street)
        {
            var response = await _streetServiceGateway.SearchAsync(new StreetSearch { StreetProvider = EStreetProvider.CRM, SearchTerm = street });
            return response.ResponseContent;
        }
    }
}
