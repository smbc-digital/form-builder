using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Services.RetrieveExternalDataService
{
    public interface IRetrieveExternalDataService
    {
        Task Process(List<PageAction> actions, string formName);
    }
}