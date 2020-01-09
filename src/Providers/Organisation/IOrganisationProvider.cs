using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.Organisation
{
    public interface IOrganisationProvider
    {
        string ProviderName { get; }

        Task<IEnumerable<StockportGovUK.NetStandard.Models.Models.Verint.Organisation>> SearchAsync(string organisation);
    }
}