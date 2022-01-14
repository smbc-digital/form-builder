using System.Collections.Generic;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;

namespace form_builder.Providers
{
    public class FormAnswersProvider : IFormAnswersProvider
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;

        public FormAnswersProvider(ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache)
        {
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
        }

        public FormAnswers GetFormAnswers()
        {
            string sessionGuid = _sessionHelper.GetSessionGuid();
            string cachedAnswers = _distributedCache.GetString(sessionGuid);
            return cachedAnswers is null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
        }
    }
}