namespace form_builder.Workflows.SuccessWorkflow;

public class SuccessWorkflow(IPageService pageService,
    ISchemaFactory schemaFactory,
    IActionsWorkflow actionsWorkflow,
    IAnalyticsService analyticsService)
    : ISuccessWorkflow
{
    public async Task<SuccessPageEntity> Process(EBehaviourType behaviourType, string form)
    {
        var baseForm = await schemaFactory.Build(form);

        if (baseForm.FormActions.Any())
            await actionsWorkflow.Process(baseForm.FormActions, baseForm, form);

        var result = await pageService.FinalisePageJourney(form, behaviourType, baseForm);

        analyticsService.RaiseEvent(form, EAnalyticsEventType.Finish);

        return result;
    }
}