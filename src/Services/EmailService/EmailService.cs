namespace form_builder.Services.EmailService;

public class EmailService(ISessionHelper sessionHelper,
    IDistributedCacheWrapper distributedCache,
    IEnumerable<IEmailProvider> emailProviders,
    IActionHelper actionHelper)
    : IEmailService
{
    private readonly IEmailProvider _emailProvider = emailProviders.First();

    public async Task Process(List<IAction> actions, string form)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string cacheKey = $"{form}::{browserSessionId}";

        if (string.IsNullOrEmpty(browserSessionId))
            throw new Exception("EmailService::Process: Session has expired");

        var formData = distributedCache.GetString(cacheKey);
        var formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

        foreach (var action in actions)
        {
            await action.Process(actionHelper, _emailProvider, formAnswers);
        }
    }
}