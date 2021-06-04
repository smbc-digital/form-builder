using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Helpers.ElementHelpers
{
    public interface IElementHelper
    {
        string CurrentValue(string questionId, Dictionary<string, dynamic> viewmodel, FormAnswers answers, string suffix = "");

        T CurrentValue<T>(string questionId, Dictionary<string, dynamic> viewmodel, FormAnswers answers, string suffix = "") where T : new();

        bool CheckForQuestionId(Element element);

        bool CheckForLabel(Element element);

        bool CheckForMaxLength(Element element);

        bool CheckIfLabelAndTextEmpty(Element element);

        bool CheckForRadioOptions(Element element);

        bool CheckForSelectOptions(Element element);

        bool CheckForCheckBoxListValues(Element element);

        bool CheckAllDateRestrictionsAreNotEnabled(Element element);

        void ReSelectPreviousSelectedOptions(Element element);

        void ReCheckPreviousRadioOptions(Element element);

        bool CheckForProvider(Element element);

        object GetFormDataValue(string guid, string key);

        FormAnswers GetFormData(string guid);

        Task<List<PageSummary>> GenerateQuestionAndAnswersList(string guid, FormSchema formSchema);

        string GenerateDocumentUploadUrl(Element element, FormSchema formSchema, FormAnswers formAnswers);
        void OrderOptionsAlphabetically(Element element);
    }
}