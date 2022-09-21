using form_builder.Constants;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class NumericValueValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Properties.Numeric || !viewModel.ContainsKey(element.Properties.QuestionId))
                return new ValidationResult { IsValid = true };

            var value = viewModel[element.Properties.QuestionId];
            if (string.IsNullOrEmpty(value) && element.Properties.Optional)
                return new ValidationResult { IsValid = true };

            var isValid = int.TryParse(value, SystemConstants.NUMERIC_NUMBER_STYLES, null, out int output);
            if (!isValid)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.NotAnIntegerValidationMessage) ? element.Properties.NotAnIntegerValidationMessage : $"{element.Properties.Label} must be a whole number"
                };
            }

            return new ValidationResult { IsValid = true };
        }
    }
}