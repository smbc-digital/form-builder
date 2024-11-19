using form_builder.Models;
using form_builder.ViewModels;

namespace form_builder.Helpers.PageHelpers;

public interface IPageHelper
{
    Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string cacheKey, FormAnswers formAnswers, List<object> results = null);

    void RemoveFieldset(Dictionary<string, dynamic> viewModel, string form, string cacheKey, string path, string removeKey);

    FormAnswers GetSavedAnswers(string cacheKey);

    Dictionary<string, dynamic> SanitizeViewModel(Dictionary<string, dynamic> viewModel);

    void SaveAnswers(Dictionary<string, dynamic> viewModel, string cacheKey, string form, IEnumerable<CustomFormFile> files, bool isPageValid, bool appendMultipleFileUploadParts = false);

    void SaveCaseReference(string cacheKey, string caseReference, bool isGenerated = false, string generatedRefereceMappingId = "");

    void SavePaymentAmount(string cacheKey, string paymentAmount, string targetMapping);

    void SaveFormData(string key, object value, string cacheKey, string formName);

    void RemoveFormData(string key, string cacheKey, string formName);

    void SaveNonQuestionAnswers(Dictionary<string, object> values, string form, string path, string cacheKey);

    List<Answers> SaveFormFileAnswers(List<Answers> answers, IEnumerable<CustomFormFile> files, bool isMultipleFileUploadElementType, PageAnswers currentAnswersForFileUpload);

    Page GetPageWithMatchingRenderConditions(List<Page> pages, string form);
}