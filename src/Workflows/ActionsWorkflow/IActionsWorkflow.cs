using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Actions;
using Microsoft.AspNetCore.Http;

namespace form_builder.Workflows.ActionsWorkflow
{
    public interface IActionsWorkflow
    {
        Task Process(List<IAction> actions, FormSchema formSchema, string formName, IQueryCollection queryParameters);
    }
}
