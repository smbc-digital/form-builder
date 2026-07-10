namespace form_builder_tests.Builders;

public class MappingEntityBuilder
{
    private FormSchema _baseForm = new();
    private FormAnswers _formAnswers = new();
    private object _data = new();

    public MappingEntity Build() => new()
    {
        BaseForm = _baseForm,
        FormAnswers = _formAnswers,
        Data = _data
    };

    public MappingEntityBuilder WithBaseForm(FormSchema baseForm)
    {
        _baseForm = baseForm;

        return this;
    }

    public MappingEntityBuilder WithFormAnswers(FormAnswers formAnswers)
    {
        _formAnswers = formAnswers;

        return this;
    }

    public MappingEntityBuilder WithData(object data)
    {
        _data = data;

        return this;
    }
}