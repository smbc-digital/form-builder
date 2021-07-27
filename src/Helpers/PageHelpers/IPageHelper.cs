using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.ViewModels;

namespace form_builder.Helpers.PageHelpers
{
    public interface IPageHelper
    {
        Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, FormAnswers formAnswers, List<object> results = null);

        void RemoveFieldset(Dictionary<string, dynamic> viewModel, string form, string guid, string path, string removeKey);

        FormAnswers GetSavedAnswers(string guid);

        void SaveAnswers(Dictionary<string, dynamic> viewModel, string guid, string form, IEnumerable<CustomFormFile> files, bool isPageValid, bool appendMultipleFileUploadParts = false);

        void SaveCaseReference(string guid, string caseReference, bool isGenerated = false, string generatedRefereceMappingId = "");

        void SavePaymentAmount(string guid, string paymentAmount, string targetMapping);

        void SaveFormData(string key, object value, string guid, string formName);

        void SaveNonQuestionAnswers(Dictionary<string, object> values, string form, string path, string guid);

        List<Answers> SaveFormFileAnswers(List<Answers> answers, IEnumerable<CustomFormFile> files, bool isMultipleFileUploadElementType, PageAnswers currentAnswersForFileUpload);

        Page GetPageWithMatchingRenderConditions(List<Page> pages);
    }
}