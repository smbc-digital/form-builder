using form_builder.Constants;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class DecimalValueValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Properties.Decimal || !viewModel.ContainsKey(element.Properties.QuestionId))
                return new ValidationResult { IsValid = true };

            var value = viewModel[element.Properties.QuestionId];
            if (string.IsNullOrEmpty(value) && element.Properties.Optional)
                return new ValidationResult { IsValid = true };

            var isValid = decimal.TryParse(value, SystemConstants.DECIMAL_NUMBER_STYLES, null, out decimal decimalOuput);
            if (!isValid)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.DecimalValidationMessage) ? element.Properties.DecimalValidationMessage : $"{element.Properties.Label} must be a valid number"
                };
            }

            var correctNumberOfDecimalSpacesEntered = value.Contains(".") ? value.Split('.')[1].Length <= element.Properties.DecimalPlaces : true;

            if (!correctNumberOfDecimalSpacesEntered)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.DecimalPlacesValidationMessage) ? element.Properties.DecimalPlacesValidationMessage : $"{element.Properties.Label} must be to {element.Properties.DecimalPlaces} decimal places or less"
                };
            }

            return new ValidationResult { IsValid = true };
        }
    }
}
