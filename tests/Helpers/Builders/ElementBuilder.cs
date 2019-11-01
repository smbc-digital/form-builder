using form_builder.Enum;
using form_builder.Models;

namespace form_builder_tests.Helpers.Builders
{
    public class ElementBuilder
    {
        private EElementType _type = EElementType.H1;
        private Property _property = new Property();

        public Element Build()
        {
            return new Element
            {
                Properties = _property,
                Type = _type
            };
        }

        public ElementBuilder WithType(EElementType type)
        {
            _type = type;
            return this;
        }

        public ElementBuilder WithPropertyText(string propertyText)
        {
            _property.Text = propertyText;
            return this;
        }
    }
}