namespace form_builder.Providers.Organisation;

public class ServiceOrganisationProvider(IOrganisationServiceGateway organisationServiceGateway) : IOrganisationProvider
{
    public string ProviderName => "CRM";

    public async Task<IEnumerable<OrganisationSearchResult>> SearchAsync(string organisation)
    {
        var result = await organisationServiceGateway.SearchAsync(new OrganisationSearch { OrganisationProvider = EOrganisationProvider.CRM, SearchTerm = organisation });

        return result.ResponseContent;
    }
}