using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Properties.ElementProperties;

namespace form_builder.Providers.Lookup
{
    public interface ILookupProvider
    {
        string ProviderName { get; }
        Task<IList<Option>> GetAsync(Source source, string query);
    }
}