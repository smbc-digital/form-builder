namespace form_builder.Workflows.ActionsWorkflow;

public class ActionsWorkflow(IRetrieveExternalDataService retrieveExternalDataService,
    IEmailService emailService,
    ISchemaFactory schemaFactory,
    IValidateService validateService,
    ITemplatedEmailService templatedEmailService)
    : IActionsWorkflow
{
    public async Task Process(List<IAction> actions, FormSchema formSchema, string formName)
    {
        if (formSchema is null)
            formSchema = await schemaFactory.Build(formName);

        if (actions.Any(_ => _.Type.Equals(EActionType.RetrieveExternalData)))
            await retrieveExternalDataService.Process(actions.Where(_ => _.Type.Equals(EActionType.RetrieveExternalData)).ToList(), formSchema, formName);

        if (actions.Any(_ => _.Type.Equals(EActionType.UserEmail) || _.Type.Equals(EActionType.BackOfficeEmail)))
            await emailService.Process(actions.Where(_ => _.Type.Equals(EActionType.UserEmail) || _.Type.Equals(EActionType.BackOfficeEmail)).ToList(), formName);

        if (actions.Any(_ => _.Type.Equals(EActionType.Validate)))
            await validateService.Process(actions.Where(_ => _.Type.Equals(EActionType.Validate)).ToList(), formSchema, formName);

        if (actions.Any(_ => _.Type.Equals(EActionType.TemplatedEmail)))
            _ = templatedEmailService.ProcessTemplatedEmail(actions.Where(_ => _.Type.Equals(EActionType.TemplatedEmail)).ToList(), formName);
    }
}