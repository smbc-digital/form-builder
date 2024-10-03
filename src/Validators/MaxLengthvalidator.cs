using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class MaxLengthValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!viewModel.ContainsKey(element.Properties.QuestionId))
                return new ValidationResult { IsValid = true };

            if (element.Type.Equals(EElementType.FileUpload) || element.Type.Equals(EElementType.Map) || element.Type.Equals(EElementType.Checkbox) || element.Type.Equals(EElementType.AddAnother))
                return new ValidationResult { IsValid = true };

            var value = viewModel.ContainsKey(element.Properties.QuestionId) ? viewModel[element.Properties.QuestionId] : "";

            if (!string.IsNullOrEmpty(value) && value.Length > element.Properties.MaxLength)
            {
                var validationMessage = element.Properties.Decimal || element.Properties.Numeric
                    ? $"{element.Properties.Label} must be {element.Properties.MaxLength} digits or less"
                     : $"Shorten the text so it's no longer than {element.Properties.MaxLength} characters";

                return new ValidationResult
                {
                    IsValid = false,
                    Message = validationMessage
                };
            }

            return new ValidationResult { IsValid = true };
        }
    }
}
