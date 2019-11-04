using form_builder.Enum;
using form_builder.Models;
using System.Collections.Generic;

namespace form_builder_tests.Builders
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

        public ElementBuilder WithQuestionId(string questionId)
        {
            _property.QuestionId = questionId;
            return this;
        }


        public ElementBuilder WithLabel(string label)
        {
            _property.Label = label;
            return this;
        }

        public ElementBuilder WithValue(string value)
        {
            _property.Label = value;
            return this;
        }

        public ElementBuilder WithListItems(List<string> listItems)
        {
            _property.ListItems = listItems;
            return this;
        }
    }
}