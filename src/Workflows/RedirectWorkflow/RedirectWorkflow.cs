using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.SubmitService;
using System;
using System.Threading.Tasks;

namespace form_builder.Workflows.RedirectWorkflow {
    public class RedirectWorkflow : IRedirectWorkflow {

        private readonly ISubmitService _submitService;
        private readonly IMappingService _mappingService;
        private readonly ISessionHelper _sessionHelper;

        public RedirectWorkflow(ISubmitService submitService, IMappingService mappingService, ISessionHelper sessionHelper)
        {
            _submitService = submitService;
            _mappingService = mappingService;
            _sessionHelper = sessionHelper;
        }

        public async Task<string> Submit(string form, string path)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new ApplicationException("A Session GUID was not provided.");

            var data = await _mappingService.Map(sessionGuid, form);
            var reference = await _submitService.RedirectSubmission(data, form, sessionGuid);

            return reference;
        }
    }
}
