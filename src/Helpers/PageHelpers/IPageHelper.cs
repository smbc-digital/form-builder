using form_builder.Models;
using form_builder.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Helpers.PageHelpers
{
    public interface IPageHelper
    {
        void HasDuplicateQuestionIDs(List<Page> pages, string formName);
        Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<object> results = null);
        void SaveAnswers(Dictionary<string, dynamic> viewModel, string guid, string form, IEnumerable<CustomFormFile> files, bool isPageValid);
        void CheckForInvalidQuestionOrTargetMappingValue(List<Page> pages, string formName);
        Task CheckForPaymentConfiguration(List<Page> pages, string formName);
        void CheckForDocumentDownload(FormSchema formSchema);
        void CheckForEmptyBehaviourSlugs(List<Page> pages, string formName);
        void CheckForCurrentEnvironmentSubmitSlugs(List<Page> pages, string formName);
        void CheckSubmitSlugsHaveAllProperties(List<Page> pages, string formName);
        void CheckForAcceptedFileUploadFileTypes(List<Page> pages, string formName);
        void SaveFormData(string key, object value, string guid);
        Dictionary<string, dynamic> AddIncomingFormDataValues(Page page, Dictionary<string, dynamic> formData);
        void CheckForIncomingFormDataValues(List<Page> Pages);
    }
}