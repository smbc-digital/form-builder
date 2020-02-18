using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Mappers
{
    public interface IElementMapper
    {
        object GetAnswerValue(IElement element, FormAnswers formAnswers);
    }
}