using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class MinMaxValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!(element.Properties.Decimal || element.Properties.Numeric) || !viewModel.ContainsKey(element.Properties.QuestionId))
                return new ValidationResult { IsValid = true };

            var value = viewModel[element.Properties.QuestionId];
            if (string.IsNullOrEmpty(value) && element.Properties.Optional)
                return new ValidationResult { IsValid = true };

            var isValid = decimal.TryParse(value, SystemConstants.DECIMAL_NUMBER_STYLES, null, out decimal decimalOuput);
           
            if (!string.IsNullOrEmpty(element.Properties.Max) && !string.IsNullOrEmpty(element.Properties.Min))
            {
                var max = int.Parse(element.Properties.Max);
                var min = int.Parse(element.Properties.Min);

                if (decimalOuput > max || decimalOuput < min)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = !string.IsNullOrEmpty(element.Properties.UpperLimitValidationMessage)
                            ? element.Properties.UpperLimitValidationMessage
                            : $"{ element.Properties.Label} must be between {min} and {max} inclusive"
                    };
                }
            }

            if (!string.IsNullOrEmpty(element.Properties.Max))
            {
                var max = int.Parse(element.Properties.Max);

                if (decimalOuput > max)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"{element.Properties.Label} must be less than or equal to {max}"
                    };
                }
            }

            if (!string.IsNullOrEmpty(element.Properties.Min))
            {
                var min = int.Parse(element.Properties.Min);
                if (decimalOuput < min)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"{element.Properties.Label} must be greater than or equal to {min}"
                    };
                }
            }

            return new ValidationResult { IsValid = true };
        }
    }
}