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

        private readonly ILogger<RedirectWorkflow> _logger;

        public RedirectWorkflow(ISubmitService submitService, IMappingService mappingService, ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache, ILogger<RedirectWorkflow> logger)
        {
            _submitService = submitService;
            _mappingService = mappingService;
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
            _logger = logger;
            
        }

        public async Task<string> Submit(string form, string path)
        {
            string browserSessionId = _sessionHelper.GetBrowserSessionId();
            string formSessionId = $"{form}::{browserSessionId}";

            if (string.IsNullOrEmpty(formSessionId))
                throw new ApplicationException("RedirectWorkflow:Submit: Session GUID is null");

            var data = await _mappingService.Map(formSessionId, form);
            var redirectUrl = await _submitService.RedirectSubmission(data, form, formSessionId);

            _logger.LogInformation($"RedirectWorkflow:Submit:{formSessionId}: Disposing session");
            _distributedCache.Remove(formSessionId);

            return redirectUrl;
        }
    }
}
