using form_builder.Models;

namespace form_builder.Helpers.DocumentCreation
{
    public interface IDocumentCreationHelper
    {
        Task<List<string>> GenerateQuestionAndAnswersList(FormAnswers formAnswers, FormSchema formSchema);
		Task<List<string>> GenerateQuestionAndAnswersListWithPageTitles(FormAnswers formAnswers, FormSchema formSchema);
		Task<List<string>> GenerateQuestionAndAnswersListForPdf(FormAnswers formAnswers, FormSchema formSchema);
    }
}
