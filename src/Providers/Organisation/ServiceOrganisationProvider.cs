using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using StockportGovUK.NetStandard.Gateways.OrganisationServiceGateway;
using StockportGovUK.NetStandard.Models.Organisation;
using StockportGovUK.NetStandard.Models.Enums;
using Microsoft.Extensions.Logging;

namespace form_builder.Providers.Organisation
{
    public class ServiceOrganisationProvider : IOrganisationProvider
    {
        public string ProviderName => "CRM";

        private readonly IOrganisationServiceGateway _organisationServiceGateway;
        private readonly ILogger<ServiceOrganisationProvider> _logger;

        public ServiceOrganisationProvider(IOrganisationServiceGateway organisationServiceGateway, ILogger<ServiceOrganisationProvider> logger)
        {
            _organisationServiceGateway = organisationServiceGateway;
            _logger = logger;
        }
        public async Task<IEnumerable<OrganisationSearchResult>> SearchAsync(string organisation)
        {
            var result = await _organisationServiceGateway.SearchAsync(new OrganisationSearch { OrganisationProvider = EOrganisationProvider.CRM, SearchTerm = organisation });

            _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(result));

            return result.ResponseContent;
        }
    }
}