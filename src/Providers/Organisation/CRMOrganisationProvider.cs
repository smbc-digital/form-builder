using StockportGovUK.NetStandard.Gateways.Models.Verint.Lookup;
using StockportGovUK.NetStandard.Gateways.VerintService;

namespace form_builder.Providers.Organisation
{
    public class CRMOrganisationProvider : IOrganisationProvider
    {
        public string ProviderName => "CRM";

        private readonly IVerintServiceGateway _verintServiceGateway;
        public CRMOrganisationProvider(IVerintServiceGateway verintServiceGateway) => _verintServiceGateway = verintServiceGateway;

        public async Task<IEnumerable<OrganisationSearchResult>> SearchAsync(string organisation)
        {
            var result = await _verintServiceGateway.SearchForOrganisationByName(organisation);

            return result.ResponseContent;
        }
    }
}