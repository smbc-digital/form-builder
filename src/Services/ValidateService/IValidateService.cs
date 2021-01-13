using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Actions;

namespace form_builder.Services.ValidateService
{
    public interface IValidateService
    {
        Task Process(List<IAction> actions, FormSchema formSchema, string formName);
    }
}
