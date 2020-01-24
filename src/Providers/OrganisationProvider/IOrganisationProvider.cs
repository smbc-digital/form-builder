using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;

namespace form_builder.Providers.Organisation
{
    public interface IOrganisationProvider
    {
        string ProviderName { get; }

        Task<IEnumerable<OrganisationSearchResult>> SearchAsync(string organisation);
    }
}