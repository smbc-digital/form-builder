using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Services.AnalyticsService;
using form_builder.Services.PageService;
using form_builder.Services.PageService.Entities;
using form_builder.Workflows.ActionsWorkflow;

namespace form_builder.Workflows.SuccessWorkflow;

public class SuccessWorkflow(
    IPageService pageService,
    ISchemaFactory schemaFactory,
    IActionsWorkflow actionsWorkflow,
    IAnalyticsService analyticsService)
    : ISuccessWorkflow
{
    private readonly IPageService _pageService = pageService;
    private readonly ISchemaFactory _schemaFactory = schemaFactory;
    private readonly IActionsWorkflow _actionsWorkflow = actionsWorkflow;
    private readonly IAnalyticsService _analyticsService = analyticsService;

    public async Task<SuccessPageEntity> Process(EBehaviourType behaviourType, string form)
    {
        var baseForm = await _schemaFactory.Build(form);

        if (baseForm.FormActions.Any())
            await _actionsWorkflow.Process(baseForm.FormActions, baseForm, form);

        var result = await _pageService.FinalisePageJourney(form, behaviourType, baseForm);

        _analyticsService.RaiseEvent(form, EAnalyticsEventType.Finish);

        return result;
    }
}