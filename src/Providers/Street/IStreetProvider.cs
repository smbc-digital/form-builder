using StockportGovUK.NetStandard.Models.Models.Verint;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.Street
{
    public interface IStreetProvider
    {
        string ProviderName { get; }

        Task<IEnumerable<StockportGovUK.NetStandard.Models.Models.Verint.Street>> SearchAsync(string street);
    }
}