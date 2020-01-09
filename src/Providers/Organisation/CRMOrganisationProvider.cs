using StockportGovUK.NetStandard.Gateways.VerintServiceGateway;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.Organisation
{
    public class CRMOrganisationProvider : IOrganisationProvider
    {
        public string ProviderName => "CRM";

        private readonly IVerintServiceGateway _verintServiceGateway;

        public CRMOrganisationProvider(IVerintServiceGateway verintServiceGateway)
        {
            _verintServiceGateway = verintServiceGateway;
        }
        public async Task<IEnumerable<StockportGovUK.NetStandard.Models.Models.Verint.Organisation>> SearchAsync(string organisation)
        {
            var result = await _verintServiceGateway.SearchForOrganisationByName(organisation);

            return result.ResponseContent;
        }
    }
}
