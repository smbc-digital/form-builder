using StockportGovUK.NetStandard.Models.Verint.Lookup;

namespace form_builder.Providers.Organisation
{
    public interface IOrganisationProvider
    {
        string ProviderName { get; }

        Task<IEnumerable<OrganisationSearchResult>> SearchAsync(string organisation);
    }
}