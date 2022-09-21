using form_builder.Models;

namespace form_builder.Providers.Lookup
{
    public interface ILookupProvider
    {
        string ProviderName { get; }
        Task<OptionsResult> GetAsync(string url, string authToken);
    }
}