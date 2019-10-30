using form_builder.Enum;
using form_builder.Validators;
using System.Collections.Generic;

namespace form_builder.Models
{
    public class Element
    {

        private ValidationResult validationResult;

        public Element()
        {
            validationResult = new ValidationResult();
        }

        public EElementType Type { get; set; }

        public Property Properties { get; set; }

        public bool DisplayAriaDescribedby
        {
            get
            {
                return Properties.Hint != string.Empty || !IsValid;
            }
        }

        public bool IsValid
        {
            get
            {
                return validationResult.IsValid;
            }
        }

        public string ValidationMessage
        {
            get
            {
                return validationResult.Message;
            }
        }

        public void Validate(Dictionary<string, string> viewModel, IEnumerable<IElementValidator> form_builder)
        {
            foreach (var validator in form_builder)
            {
                var result = validator.Validate(this, viewModel);
                if (!result.IsValid)
                {
                    validationResult = result;
                    return;
                }
            }
        }

        public Dictionary<string, object> GenerateElementProperties()
        {
            var properties = new Dictionary<string, object>();
            //maxLength is being decided by UX. 500 is used as a temp number
            var maxLength = string.IsNullOrEmpty(Properties.MaxLength) ? "500" : Properties.MaxLength;

            switch (Type)
            {
                case EElementType.Textbox:
                case EElementType.Textarea:
                    properties = new Dictionary<string, object>()
                    {
                        {"name", Properties.QuestionId },
                        { "id", Properties.QuestionId },
                        { "maxlength", maxLength },
                        { "value", Properties.Value}
                    };

                    if (DisplayAriaDescribedby)
                    {
                        properties.Add("aria-describedby", DescribedByValue());
                    }

                    return properties;
                default:
                    return null;
            }

            return properties;
        }

        public Dictionary<string, object> GenerateElementProperties(int index)
        {
            var properties = new Dictionary<string, object>();

            switch (Type)
            {
                case EElementType.Radio:
                    properties = new Dictionary<string, object>()
                    {
                        {"name", Properties.QuestionId },
                        { "id", $"{Properties.QuestionId}-{index}" },
                        { "value", Properties.Value}
                    };

                    if (!string.IsNullOrEmpty(Properties.Options[index].Hint))
                    {
                        properties.Add("aria-describedby", $"{Properties.QuestionId}-{index}-hint");
                    }

                    return properties;
                default:
                    return null;
            }

            return properties;
        }

        public string DescribedByValue()
        {
            var describedByValue = string.Empty;

            if (!IsValid)
            {
                describedByValue += $"{Properties.QuestionId}-error ";
            }

            if (!string.IsNullOrEmpty(Properties.Hint))
            {
                describedByValue += $"{Properties.QuestionId}-hint"; ;
            }

            return describedByValue;
        }
    }
}