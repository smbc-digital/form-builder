using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
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
            _property.Value = value;
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

        public ElementBuilder WithChecked(bool isChecked)
        {
            _property.Checked = isChecked;
            return this;
        }

        public ElementBuilder WithRestrictFutureDate(bool value)
        {
            _property.RestrictFutureDate = value;
            return this;
        }

        public ElementBuilder WithRestrictPastDate(bool value)
        {
            _property.RestrictPastDate = value;
            return this;
        }

        public ElementBuilder WithRestrictCurrentDate(bool value)
        {
            _property.RestrictCurrentDate = value;
            return this;
        }

        public ElementBuilder WithEmail(bool value)
        {
            _property.Email = value;
            return this;
        }

        public ElementBuilder WithOptional(bool value)
        {
            _property.Optional = value;
            return this;
        }

        public ElementBuilder WithDayValue(string value)
        {
            _property.Day = value;
            return this;
        }

        public ElementBuilder WithMonthValue(string value)
        {
            _property.Month = value;
            return this;
        }

        public ElementBuilder WithYearValue(string value)
        {
            _property.Year = value;
            return this;
        }

        public ElementBuilder WithAddressProvider(string value)
        {
            _property.AddressProvider = value;
            return this;
        }

        public ElementBuilder WithStreetProvider(string value)
        {
            _property.StreetProvider = value;
            return this;
        }
        public ElementBuilder WithNumeric(bool value)
        {
            _property.Numeric = value;
            return this;
        }

        public ElementBuilder WithHint(string value)
        {
            _property.Hint = value;
            return this;
        }

        public  ElementBuilder WithRegex(string value)
        {
            _property.Regex = value;
            return this;
        }

        public ElementBuilder WithRegexValidationMessage(string value)
        {
            _property.RegexValidationMessage = value;
            return this;
        }
    }
}