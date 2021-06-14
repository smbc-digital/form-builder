using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Factories.Schema;
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

        public SuccessWorkflow(IPageService pageService, ISchemaFactory schemaFactory, IActionsWorkflow actionsWorkflow)
        {
            _pageService = pageService;
            _schemaFactory = schemaFactory;
            _actionsWorkflow = actionsWorkflow;
        }

        public async Task<SuccessPageEntity> Process(EBehaviourType behaviourType, string form)
        {
            var baseForm = await _schemaFactory.Build(form, string.Empty);

            if (baseForm.FormActions.Any())
                await _actionsWorkflow.Process(baseForm.FormActions, baseForm, form);

            return await _pageService.FinalisePageJourney(form, behaviourType, baseForm);
        }
    }
}