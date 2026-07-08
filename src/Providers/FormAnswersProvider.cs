using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;

namespace form_builder.Providers;

public class FormAnswersProvider(ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache)
    : IFormAnswersProvider
{
    private readonly ISessionHelper _sessionHelper = sessionHelper;
    private readonly IDistributedCacheWrapper _distributedCache = distributedCache;

    public FormAnswers GetFormAnswers(string form)
    {
        string browserSessionId = _sessionHelper.GetBrowserSessionId();
        string cacheKey = $"{form}::{browserSessionId}";
        string cachedAnswers = _distributedCache.GetString(cacheKey);
        return cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
    }
}