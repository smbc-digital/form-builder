using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Services.PageService.Entities;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Http;

namespace form_builder.Services.PageService
{
    public interface IPageService
    {
        Task<ProcessPageEntity> ProcessPage(string form, string path, string subPath, IQueryCollection queryParamters);

        Task<ProcessRequestEntity> ProcessRequest(string form, string path, Dictionary<string, dynamic> viewModel, IEnumerable<CustomFormFile> file, bool modelStateIsValid);

        Task<FormBuilderViewModel> GetViewModel(Page page, FormSchema baseForm, string path, string sessionGuid, string subPath, List<object> results);

        Behaviour GetBehaviour(ProcessRequestEntity currentPageResult);

        Task<SuccessPageEntity> FinalisePageJourney(string form, EBehaviourType behaviourType, FormSchema formSchema);

        Task<SuccessPageEntity> GetCancelBookingSuccessPage(string form);
    }
}