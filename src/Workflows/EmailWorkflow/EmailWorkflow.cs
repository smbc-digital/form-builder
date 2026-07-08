using form_builder.Helpers.Session;
using form_builder.Services.EmailSubmitService;
using form_builder.Services.MappingService;


namespace form_builder.Workflows.EmailWorkflow;

public class EmailWorkflow(
    IEmailSubmitService emailSubmitService,
    IMappingService mappingService,
    ISessionHelper sessionHelper)
    : IEmailWorkflow
{
    private readonly IEmailSubmitService _emailSubmitService = emailSubmitService;
    private readonly IMappingService _mappingService = mappingService;
    private readonly ISessionHelper _sessionHelper = sessionHelper;

    public async Task<string> Submit(string form)
    {
        string browserSessionId = _sessionHelper.GetBrowserSessionId();
        if (string.IsNullOrEmpty(browserSessionId))
            throw new ApplicationException("A Session GUID was not provided.");

        string formSessionId = $"{form}::{browserSessionId}";
            
        var data = await _mappingService.Map(formSessionId, form, null, null);

        return await _emailSubmitService.EmailSubmission(data, form, formSessionId);
    }
}