namespace form_builder.Mappers.Element;

public interface IElementMapper
{
    Task<T> GetAnswerValue<T>(IElement element, FormAnswers formAnswers);

    Task<object> GetAnswerValue(IElement element, FormAnswers formAnswers);

    Task<string> GetAnswerStringValue(IElement question, FormAnswers formAnswers);
}