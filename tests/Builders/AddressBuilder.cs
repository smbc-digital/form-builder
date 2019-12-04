using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder_tests.Builders
{
    public class AddressBuilder
    {
        private Property _property = new Property();

        public Address Build()
        {
            return new Address
            {
                Properties = _property,
            };
        }

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
