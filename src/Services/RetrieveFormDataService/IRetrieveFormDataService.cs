using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Actions;

namespace form_builder.Services.RetrieveFormDataService
{
    public interface IRetrieveFormDataService
    {
        Task Process(List<IAction> actions, FormSchema formSchema, string formName);
    }
}
