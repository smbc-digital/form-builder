namespace form_builder.Workflows.RedirectWorkflow;

public class RedirectWorkflow(ISubmitService submitService,
    IMappingService mappingService,
    ISessionHelper sessionHelper,
    IDistributedCacheWrapper distributedCache,
    ILogger<RedirectWorkflow> logger)
    : IRedirectWorkflow
{
    public async Task<string> Submit(string form, string path)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        if (string.IsNullOrEmpty(browserSessionId))
            throw new ApplicationException("RedirectWorkflow:Submit: Session GUID is null");

        string formSessionId = $"{form}::{browserSessionId}";

        var data = await mappingService.Map(formSessionId, form, null, null);
        var redirectUrl = await submitService.RedirectSubmission(data, form, formSessionId);

        logger.LogInformation($"RedirectWorkflow:Submit:{formSessionId}: Disposing session");
        distributedCache.Remove(formSessionId);

        return redirectUrl;
    }
}