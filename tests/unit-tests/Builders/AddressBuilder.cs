using Address = form_builder.Models.Elements.Address;

namespace form_builder_tests.Builders;

public class AddressBuilder
{
    private readonly BaseProperty _property = new();

    public Address Build() => new()
    {
        Properties = _property,
    };

    public AddressBuilder WithPropertyText(string propertyText)
    {
        _property.Text = propertyText;

        return this;
    }

    public AddressBuilder WithQuestionId(string questionId)
    {
        _property.QuestionId = questionId;

        return this;
    }
}