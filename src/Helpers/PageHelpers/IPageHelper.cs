using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.ViewModels;

namespace form_builder.Helpers.PageHelpers
{
    public interface IPageHelper
    {
        void HasDuplicateQuestionIDs(List<Page> pages, string formName);
        
        Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, FormAnswers formAnswers, List<object> results = null);
        
        void SaveAnswers(Dictionary<string, dynamic> viewModel, string guid, string form, IEnumerable<CustomFormFile> files, bool isPageValid, bool appendMultipleFileUploadParts = false);

        void SaveCaseReference(string guid, string caseReference);

        void CheckForInvalidQuestionOrTargetMappingValue(List<Page> pages, string formName);
        
        Task CheckForPaymentConfiguration(List<Page> pages, string formName);
        
        void CheckForDocumentDownload(FormSchema formSchema);
        
        void CheckForEmptyBehaviourSlugs(List<Page> pages, string formName);
        
        void CheckForCurrentEnvironmentSubmitSlugs(List<Page> pages, string formName);
        
        void CheckSubmitSlugsHaveAllProperties(List<Page> pages, string formName);
        
        void CheckForAcceptedFileUploadFileTypes(List<Page> pages, string formName);
        
        void SaveFormData(string key, object value, string guid);
        
        void SaveNonQuestionAnswers(Dictionary<string, object> values, string form, string path, string guid);

        void CheckForIncomingFormDataValues(List<Page> pages);
        
        void CheckForPageActions(FormSchema formSchema);
        
        void CheckRenderConditionsValid(List<Page> pages);
        
        Page GetPageWithMatchingRenderConditions(List<Page> pages);

        void CheckAddressNoManualTextIsSet(List<Page> pages);

        void CheckForAnyConditionType(List<Page> pages);

        List<Answers> SaveFormFileAnswers(List<Answers> answers, IEnumerable<CustomFormFile> files, bool isMultipleFileUploadElementType, PageAnswers currentAnswersForFileUpload);
    }
}