using form_builder.Models;
using form_builder.Models.Elements;
using System.Threading.Tasks;

namespace form_builder.Mappers
{
    public interface IElementMapper
    {
        Task<T> GetAnswerValue<T>(IElement element, FormAnswers formAnswers);

        Task<object> GetAnswerValue(IElement element, FormAnswers formAnswers);

        Task<string> GetAnswerStringValue(IElement question, FormAnswers formAnswers);
    }
}
