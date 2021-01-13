using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.Helpers.DocumentCreation
{
    public interface IDocumentCreationHelper
    {
        List<string> GenerateQuestionAndAnswersList(FormAnswers formAnswers, FormSchema formSchema);
    }
}
