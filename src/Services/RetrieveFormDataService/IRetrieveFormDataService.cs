using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Actions;
using Microsoft.AspNetCore.Http;

namespace form_builder.Services.RetrieveFormDataService
{
    public interface IRetrieveFormDataService
    {
        Task Process(IAction action, FormSchema formSchema, string formName, IQueryCollection queryParameters);
    }
}
