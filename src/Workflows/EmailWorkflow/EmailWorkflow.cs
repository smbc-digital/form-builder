using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.EmailSubmitService;


namespace form_builder.Workflows.EmailWorkflow
{
    public class EmailWorkflow : IEmailWorkflow
    {
        private readonly IEmailSubmitService _emailSubmitService;
        private readonly IMappingService _mappingService;
        private readonly ISessionHelper _sessionHelper;
        public EmailWorkflow(
            IEmailSubmitService emailSubmitService,
            IMappingService mappingService,
            ISessionHelper sessionHelper
            )
        {
            _emailSubmitService = emailSubmitService;
            _mappingService = mappingService;
            _sessionHelper = sessionHelper;
        }

        public async Task<string> Submit(string form)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new ApplicationException($"A Session GUID was not provided.");

            var data = await _mappingService.Map(sessionGuid, form);

            return await _emailSubmitService.EmailSubmission(data, form, sessionGuid);
        }
    }
}
