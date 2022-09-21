using form_builder.Models;
using form_builder.Models.Actions;

namespace form_builder.Workflows.ActionsWorkflow
{
    public interface IActionsWorkflow
    {
        Task Process(List<IAction> actions, FormSchema formSchema, string formName);
    }
}
