using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;

namespace form_builder_tests.Builders
{
    public class AddressBuilder
    {
        private readonly BaseProperty _property = new BaseProperty();

        public Address Build() => new Address
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
}