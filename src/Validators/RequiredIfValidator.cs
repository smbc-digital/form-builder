using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class RequiredIfValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (string.IsNullOrEmpty(element.Properties.RequiredIf))
                return new ValidationResult { IsValid = true };

            var isValid = true;
            var requiredIf = element.Properties.RequiredIf.Split(':');
            var requiredKey = requiredIf[0];

            if (!viewModel.TryGetValue(requiredKey, out dynamic answeredValue))
                return new ValidationResult { IsValid = true };

            answeredValue = viewModel[requiredKey];

            if (answeredValue.Equals(requiredIf[1]))
            {
                if (element.Type.Equals(EElementType.Textarea) || element.Type.Equals(EElementType.Textbox))
                {
                    if (viewModel[element.Properties.QuestionId].Equals(string.Empty) || viewModel[element.Properties.QuestionId] is null) isValid = false;
                }
                else
                {
                    if (viewModel.TryGetValue(element.Properties.QuestionId, out dynamic value))
                    {
                        isValid = true;
                        if (value.Equals(string.Empty)) isValid = false;
                    }
                    else
                    {
                        isValid = false;
                    }
                }
            }

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty
                    : !string.IsNullOrEmpty(element.Properties.RequiredIfValidationMessage)
                    ? element.Properties.RequiredIfValidationMessage : $"Check the {element.Properties.Label} and try again."
            };
        }
    }
}