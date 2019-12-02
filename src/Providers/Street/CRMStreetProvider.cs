using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Providers.Street;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;

namespace form_builder.Providers.Address
{
    public class CRMStreetProvider : IStreetProvider
    {
        public string ProviderName => "CRMStreet";

        private readonly IVerintServiceGateway _verintServiceGateway;

        public CRMStreetProvider(IVerintServiceGateway verintServiceGateway)
        {
            _verintServiceGateway = verintServiceGateway;
        }

        public async Task<IEnumerable<StockportGovUK.NetStandard.Models.Models.Verint.Street>> SearchAsync(string street)
        {
            var response = await _verintServiceGateway.GetStreetByReference(street);
            return response.ResponseContent;
        }
    }
}
