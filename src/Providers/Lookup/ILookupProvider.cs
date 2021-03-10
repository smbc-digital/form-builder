using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Providers.Lookup
{
    public interface ILookupProvider
    {
        string ProviderName { get; }
        Task<IList<Option>> GetAsync(string key);
    }
}