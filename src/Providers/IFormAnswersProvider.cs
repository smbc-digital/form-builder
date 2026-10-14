namespace form_builder.Providers;

public interface IFormAnswersProvider
{
    FormAnswers GetFormAnswers(string form);
}