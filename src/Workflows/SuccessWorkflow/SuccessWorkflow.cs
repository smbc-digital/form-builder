using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Helpers.Session;
using form_builder.Services.PageService;
using form_builder.Services.PageService.Entities;
using form_builder.Workflows.ActionsWorkflow;

namespace form_builder.Workflows.SuccessWorkflow
{
    public class SuccessWorkflow : ISuccessWorkflow
    {
        private readonly IPageService _pageService;
        private readonly ISchemaFactory _schemaFactory;
        private readonly IActionsWorkflow _actionsWorkflow;
        private readonly ISessionHelper _sessionHelper;

        public SuccessWorkflow(IPageService pageService, ISchemaFactory schemaFactory, IActionsWorkflow actionsWorkflow, ISessionHelper sessionHelper)
        {
            _pageService = pageService;
            _schemaFactory = schemaFactory;
            _actionsWorkflow = actionsWorkflow;
            _sessionHelper = sessionHelper;
        }

        public async Task<SuccessPageEntity> Process(EBehaviourType behaviourType, string form)
        {
            var baseForm = await _schemaFactory.Build(form);
            var sessionGuid = _sessionHelper.GetSessionGuid();

            foreach (var page in baseForm.Pages)
                await _schemaFactory.TransformPage(page, sessionGuid);

            if (baseForm.FormActions.Any())
                await _actionsWorkflow.Process(baseForm.FormActions, baseForm, form);

            return await _pageService.FinalisePageJourney(form, behaviourType, baseForm);
        }
    }
}