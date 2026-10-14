namespace form_builder.Workflows.SubmitWorkflow;

public class SubmitWorkflow(ISubmitService submitService,
    IMappingService mappingService,
    ISessionHelper sessionHelper)
    : ISubmitWorkflow
{
    public async Task<string> Submit(string form)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        if (string.IsNullOrEmpty(browserSessionId))
            throw new ApplicationException("A Session GUID was not provided.");

        string formSessionId = $"{form}::{browserSessionId}";

        await submitService.PreProcessSubmission(form, formSessionId);

        var data = await mappingService.Map(formSessionId, form, null, null);

        return await submitService.ProcessSubmission(data, form, formSessionId);
    }

    public async Task SubmitWithoutSubmission(string form)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        if (string.IsNullOrEmpty(browserSessionId))
            throw new ApplicationException("A Session GUID was not provided.");

        string formSessionId = $"{form}::{browserSessionId}";

        await submitService.PreProcessSubmission(form, formSessionId);

        var data = await mappingService.Map(formSessionId, form, null, null);

        await submitService.ProcessWithoutSubmission(data, form, formSessionId);
    }
}