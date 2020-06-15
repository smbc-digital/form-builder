using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StockportGovUK.NetStandard.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Addresses;

namespace form_builder.Providers.Address
{
    public class CRMAddressProvider : IAddressProvider
    {
        public string ProviderName => "CRM";

        private readonly IVerintServiceGateway _verintServiceGateway;
        private readonly ILogger<IAddressProvider> _logger;

        public CRMAddressProvider(IVerintServiceGateway verintServiceGateway, ILogger<IAddressProvider> logger)
        {
            _verintServiceGateway = verintServiceGateway;
            _logger = logger;
        }

        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode)
        {
            var response = await _verintServiceGateway.SearchForPropertyByPostcode(streetOrPostcode);

            _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(response));

            return response.ResponseContent ?? new List<AddressSearchResult>();
        }
    }
}
