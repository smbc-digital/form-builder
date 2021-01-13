using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Mappers
{
    public interface IElementMapper
    {
        T GetAnswerValue<T>(IElement element, FormAnswers formAnswers);

        object GetAnswerValue(IElement element, FormAnswers formAnswers);

        string GetAnswerStringValue(IElement question, FormAnswers formAnswers);
    }
}
