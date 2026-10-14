namespace form_builder.Workflows.EmailWorkflow;

public class EmailWorkflow(IEmailSubmitService emailSubmitService,
    IMappingService mappingService,
    ISessionHelper sessionHelper)
    : IEmailWorkflow
{
    public async Task<string> Submit(string form)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        if (string.IsNullOrEmpty(browserSessionId))
            throw new ApplicationException("A Session GUID was not provided.");

        string formSessionId = $"{form}::{browserSessionId}";
            
        var data = await mappingService.Map(formSessionId, form, null, null);

        return await emailSubmitService.EmailSubmission(data, form, formSessionId);
    }
}