namespace form_builder.Providers;

public class FormAnswersProvider(ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache)
    : IFormAnswersProvider
{
    public FormAnswers GetFormAnswers(string form)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string cacheKey = $"{form}::{browserSessionId}";
        string cachedAnswers = distributedCache.GetString(cacheKey);
        return cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
    }
}