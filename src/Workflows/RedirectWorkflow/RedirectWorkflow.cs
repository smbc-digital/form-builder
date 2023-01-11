using form_builder.Helpers.Session;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService;
using form_builder.Services.SubmitService;

namespace form_builder.Workflows.RedirectWorkflow
{
    public class RedirectWorkflow : IRedirectWorkflow
    {

        private readonly ISubmitService _submitService;
        private readonly IMappingService _mappingService;
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;

        public RedirectWorkflow(ISubmitService submitService, IMappingService mappingService, ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache)
        {
            _submitService = submitService;
            _mappingService = mappingService;
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
        }

        public async Task<string> Submit(string form, string path)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new ApplicationException("A Session GUID was not provided.");

            var data = await _mappingService.Map(sessionGuid, form);
            var redirectUrl = await _submitService.RedirectSubmission(data, form, sessionGuid);

            _distributedCache.Remove(sessionGuid);

            return redirectUrl;
        }
    }
}
