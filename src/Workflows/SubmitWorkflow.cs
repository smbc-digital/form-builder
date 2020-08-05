using System;
using System.Threading.Tasks;
using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.SubmtiService;

namespace form_builder.Workflows
{
    public interface ISubmitWorkflow
    {
        Task<string> Submit(string form);
    }

    public class SubmitWorkflow : ISubmitWorkflow
    {
        private readonly ISubmitService _submitService;
        private readonly IMappingService _mappingService;
        private readonly ISessionHelper _sessionHelper;

        public SubmitWorkflow(ISubmitService submitService, IMappingService mappingService, ISessionHelper sessionHelper)
        {
            _submitService = submitService;
            _mappingService = mappingService;
            _sessionHelper = sessionHelper;
        }

        public async Task<string> Submit(string form)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new ApplicationException($"A Session GUID was not provided.");

            var data = await _mappingService.Map(sessionGuid, form);

            return await _submitService.ProcessSubmission(data, form, sessionGuid);
        }
    }
}