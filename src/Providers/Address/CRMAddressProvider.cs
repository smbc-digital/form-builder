using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Addresses;

namespace form_builder.Providers.Address
{
    public class CRMAddressProvider : IAddressProvider
    {
        public string ProviderName => "CRM";

        private readonly IVerintServiceGateway _verintServiceGateway;

        public CRMAddressProvider(IVerintServiceGateway verintServiceGateway)
        {
            _verintServiceGateway = verintServiceGateway;
        }

        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string postcode)
        {
            var response = await _verintServiceGateway.SearchForPropertyByPostcode(postcode);
            return response.ResponseContent;
        }
    }
}
