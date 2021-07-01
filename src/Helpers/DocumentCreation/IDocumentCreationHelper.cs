using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Helpers.DocumentCreation
{
    public interface IDocumentCreationHelper
    {
        Task<List<string>> GenerateQuestionAndAnswersList(FormAnswers formAnswers, FormSchema formSchema);
    }
}
