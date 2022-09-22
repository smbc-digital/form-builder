using StockportGovUK.NetStandard.Gateways.OrganisationService;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.Models.Organisation;
using StockportGovUK.NetStandard.Gateways.Models.Verint.Lookup;

namespace form_builder.Providers.Organisation
{
    public class ServiceOrganisationProvider : IOrganisationProvider
    {
        public string ProviderName => "CRM";

        private readonly IOrganisationServiceGateway _organisationServiceGateway;
        public ServiceOrganisationProvider(IOrganisationServiceGateway organisationServiceGateway) => _organisationServiceGateway = organisationServiceGateway;

        public async Task<IEnumerable<OrganisationSearchResult>> SearchAsync(string organisation)
        {
            var result = await _organisationServiceGateway.SearchAsync(new OrganisationSearch { OrganisationProvider = EOrganisationProvider.CRM, SearchTerm = organisation });

            return result.ResponseContent;
        }
    }
}