using form_builder.Enum;
using form_builder.Models;
using System;
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

        public ElementBuilder WithSource(string source)
        {
            _property.Source = source;
            return this;
        }

        public ElementBuilder WithAltText(string alt)
        {
            _property.AltText = alt;
            return this;
        }


        public ElementBuilder WithMaxLength(int maxLength)
        {
            _property.MaxLength = maxLength.ToString();
            return this;
        }

        public ElementBuilder WithOptions(List<Option> options)
        {
            _property.Options = options;
            return this;
        }
    }
}