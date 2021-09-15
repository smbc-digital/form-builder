using System;
using System.Threading.Tasks;
using form_builder.Helpers.Session;
using form_builder.Providers.Analytics.Request;
using form_builder.Services.AnalyticsService;
using form_builder.Services.MappingService;
using form_builder.Services.SubmitService;

namespace form_builder.Workflows.SubmitWorkflow
{
    public class SubmitWorkflow : ISubmitWorkflow
    {
        private readonly ISubmitService _submitService;
        private readonly IMappingService _mappingService;
        private readonly IAnalyticsService _analyticsService;
        private readonly ISessionHelper _sessionHelper;

        public SubmitWorkflow(ISubmitService submitService, 
            IMappingService mappingService, 
            IAnalyticsService analyticsService, 
            ISessionHelper sessionHelper)
        {
            _submitService = submitService;
            _mappingService = mappingService;
            _analyticsService = analyticsService;
            _sessionHelper = sessionHelper;
        }

        public async Task<string> Submit(string form)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new ApplicationException($"A Session GUID was not provided.");

            await _submitService.PreProcessSubmission(form, sessionGuid);

            var data = await _mappingService.Map(sessionGuid, form);

            var result = await _submitService.ProcessSubmission(data, form, sessionGuid);

            _analyticsService.RaiseEvent(form, EAnalyticsEventType.Finish);

            return result;
        }
    }
}