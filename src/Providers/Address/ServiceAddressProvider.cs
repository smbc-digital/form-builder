using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Gateways.AddressService;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Enums;

namespace form_builder.Providers.Address
{
    public class ServiceAddressProvider : IAddressProvider
    {
        public string ProviderName => "CRM";

        private readonly IAddressServiceGateway _addressServiceGateway;

        public ServiceAddressProvider(IAddressServiceGateway addressServiceGateway)
        {
            _addressServiceGateway = addressServiceGateway;
        }

        public async Task<IEnumerable<AddressSearchResult>> SearchAsync(string streetOrPostcode)
        {
            var response = await _addressServiceGateway.SearchAsync(new AddressSearch { AddressProvider = EAddressProvider.CRM, SearchTerm = streetOrPostcode });
            return response.ResponseContent;
        }
    }
}
