using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Actions;

namespace form_builder.Services.RetrieveExternalDataService
{
    public interface IRetrieveExternalDataService
    {
        Task Process(List<IAction> actions, FormSchema formSchema, string formName);
    }
}