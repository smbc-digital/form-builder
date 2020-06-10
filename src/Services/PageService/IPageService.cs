using System.Collections.Generic;
using form_builder.Models;
using System.Threading.Tasks;
using form_builder.Services.PageService.Entities;
using form_builder.Enum;
using form_builder.ViewModels;

namespace form_builder.Services.PageService
{
    public interface IPageService
    {
        Task<ProcessPageEntity> ProcessPage(string form, string path, bool isAddressManual = false);
        Task<ProcessRequestEntity> ProcessRequest(string form, string path, Dictionary<string, dynamic> viewModel, IEnumerable<CustomFormFile> file, bool processManual = false);
        Task<FormBuilderViewModel> GetViewModel(Page page, FormSchema baseForm, string path, string sessionGuid);
        Behaviour GetBehaviour(ProcessRequestEntity currentPageResult);
        Task<SuccessPageEntity> FinalisePageJourney(string form, EBehaviourType behaviourType);
    }
}