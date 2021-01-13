using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Gateways.OrganisationService;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Organisation;
using StockportGovUK.NetStandard.Models.Verint.Lookup;

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