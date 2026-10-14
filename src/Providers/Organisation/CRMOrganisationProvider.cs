namespace form_builder.Providers.Organisation;

public class CRMOrganisationProvider(IVerintServiceGateway verintServiceGateway) : IOrganisationProvider
{
    public string ProviderName => "CRM";

    public async Task<IEnumerable<OrganisationSearchResult>> SearchAsync(string organisation)
    {
        var result = await verintServiceGateway.SearchForOrganisationByName(organisation);

        return result;
    }
}