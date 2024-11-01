using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.SubmitService;

namespace form_builder.Workflows.SubmitWorkflow
{
    public class SubmitWorkflow : ISubmitWorkflow
    {
        private readonly ISubmitService _submitService;
        private readonly IMappingService _mappingService;
        private readonly ISessionHelper _sessionHelper;

        public SubmitWorkflow(ISubmitService submitService,
            IMappingService mappingService,
            ISessionHelper sessionHelper)
        {
            _submitService = submitService;
            _mappingService = mappingService;
            _sessionHelper = sessionHelper;
        }

        public async Task<string> Submit(string form)
        {
            ISession browserSessionId = _sessionHelper.GetSession();
            string formSessionId = $"{form}::{browserSessionId.Id}";

            if (string.IsNullOrEmpty(formSessionId))
                throw new ApplicationException("A Session GUID was not provided.");

            await _submitService.PreProcessSubmission(form, formSessionId);

            var data = await _mappingService.Map(formSessionId, form);

            return await _submitService.ProcessSubmission(data, form, formSessionId);
        }

        public async Task SubmitWithoutSubmission(string form)
        {
            ISession browserSessionId = _sessionHelper.GetSession();
            string formSessionId = $"{form}::{browserSessionId.Id}";

            if (string.IsNullOrEmpty(formSessionId))
                throw new ApplicationException("A Session GUID was not provided.");

            await _submitService.PreProcessSubmission(form, formSessionId);

            var data = await _mappingService.Map(formSessionId, form);

            await _submitService.ProcessWithoutSubmission(data, form, formSessionId);
        }
    }
}