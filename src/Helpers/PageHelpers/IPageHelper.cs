using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.ViewModels;

namespace form_builder.Helpers.PageHelpers
{
    public interface IPageHelper
    {
        Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, FormAnswers formAnswers, List<object> results = null);

        void SaveAnswers(Dictionary<string, dynamic> viewModel, string guid, string form, IEnumerable<CustomFormFile> files, bool isPageValid, bool appendMultipleFileUploadParts = false);

        void SaveCaseReference(string guid, string caseReference, bool isGenerated = false, string generatedRefereceMappingId = "");
        
        void SaveFormData(string key, object value, string guid, string formName);

        void SaveNonQuestionAnswers(Dictionary<string, object> values, string form, string path, string guid);

        List<Answers> SaveFormFileAnswers(List<Answers> answers, IEnumerable<CustomFormFile> files, bool isMultipleFileUploadElementType, PageAnswers currentAnswersForFileUpload);

        Page GetPageWithMatchingRenderConditions(List<Page> pages);

        // void HasDuplicateQuestionIDs(List<Page> pages, string formName);
        
        // void CheckForInvalidQuestionOrTargetMappingValue(List<Page> pages, string formName);

        // Task CheckForPaymentConfiguration(List<Page> pages, string formName);

        // void CheckForDocumentDownload(FormSchema formSchema);

        // void CheckForEmptyBehaviourSlugs(List<Page> pages, string formName);

        // void CheckForCurrentEnvironmentSubmitSlugs(List<Page> pages, string formName);

        // void CheckSubmitSlugsHaveAllProperties(List<Page> pages, string formName);

        // void CheckForAcceptedFileUploadFileTypes(List<Page> pages, string formName);

        // void CheckForIncomingFormDataValues(List<Page> pages);

        // void CheckForPageActions(FormSchema formSchema);

        // void CheckRenderConditionsValid(List<Page> pages);

        // void CheckAddressNoManualTextIsSet(List<Page> pages);

        // void CheckForAnyConditionType(List<Page> pages);

        // void CheckUploadedFilesSummaryQuestionsIsSet(List<Page> pages);
        
        // void CheckForBookingElement(List<Page> pages);

        // void CheckConditionalElementsAreValid(List<Page> pages, string formName);

        // void CheckQuestionIdExistsForBookingCustomerAddressId(List<Page> pages, string formName);

        // void CheckGeneratedIdConfiguration(FormSchema formSchema);

        // void CheckDateValidations(List<Page> pages);
        
        // void CheckAbsoluteDateValidations(List<Page> pages);
    }
}