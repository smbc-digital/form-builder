using form_builder.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Providers.DynamicLookupData
{
    public interface IDynamicLookDataProvider
    {
        string ProviderName { get; }

        Task<IList<Option>> GetAsync(string key);
    }
}
