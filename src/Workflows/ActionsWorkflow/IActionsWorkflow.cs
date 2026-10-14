namespace form_builder.Workflows.ActionsWorkflow;

public interface IActionsWorkflow
{
    Task Process(List<IAction> actions, FormSchema formSchema, string formName);
}