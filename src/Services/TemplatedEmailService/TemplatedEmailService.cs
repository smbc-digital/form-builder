namespace form_builder.Services.TemplatedEmailService;

public class TemplatedEmailService(IEnumerable<ITemplatedEmailProvider> templatedEmailProviders,
    IActionHelper actionHelper,
    ISessionHelper sessionHelper,
    IDistributedCacheWrapper distributedCache)
    : ITemplatedEmailService
{
    public Task ProcessTemplatedEmail(List<IAction> actions, string form)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string formSessionId = $"{form}::{browserSessionId}";

        var formData = distributedCache.GetString(formSessionId);

        if (string.IsNullOrEmpty(formData))
            throw new Exception("TemplatedEmailService::Process: Session has expired");

        var formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

        foreach (var action in actions)
        {
            var templatedEmailProvider = templatedEmailProviders.Get(action.Properties.EmailTemplateProvider);
            var convertedAnswers = formAnswers.AllAnswers.ToDictionary(x => x.QuestionId, x => x.Response);
            var personalisation = new Dictionary<string, dynamic>();

            if (action.Properties.IncludeCaseReference)
                personalisation.Add("reference", formAnswers.CaseReference);

            if (action.Properties.Personalisation is not null && !convertedAnswers.Count.Equals(0))
                action.Properties.Personalisation.ForEach(field => { personalisation.Add(field, convertedAnswers[field]); });

            action.ProcessTemplatedEmail(
                actionHelper,
                templatedEmailProvider,
                personalisation,
                formAnswers);
        }
        return null;
    }
}