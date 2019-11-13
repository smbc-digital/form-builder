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
            var maxLength = string.IsNullOrEmpty(Properties.MaxLength) ? "200" : Properties.MaxLength;

            switch (Type)
            {
                case EElementType.Textbox:
                case EElementType.Textarea:
                    var properties = new Dictionary<string, object>()
                    {
                        { "name", Properties.QuestionId },
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
        }

        public Dictionary<string, object> GenerateElementProperties(int index)
        {
            switch (Type)
            {
                case EElementType.Radio:
                    var properties = new Dictionary<string, object>()
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
        }

        public string DescribedByValue()
        {
            var describedByValue = string.Empty;

            if (!string.IsNullOrEmpty(Properties.Hint))
            {
                describedByValue += $"{Properties.QuestionId}-hint ";
            }

            if (!IsValid)
            {
                describedByValue += $"{Properties.QuestionId}-error";
            }

            return describedByValue;
        }

        public string WriteOptional()
        {
            if (DisplayOptional)
            {
                return "class = optional";
            }

            return null;
        }

        private bool DisplayOptional
        {
            get
            {
                return Properties?.Optional ?? true;
            }
        }
    }
}