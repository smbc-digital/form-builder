using System.Collections.Generic;
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

            var isValid = int.TryParse(value, out int output);
            if (!isValid)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.NotAnIntegerValidationMessage) ? element.Properties.NotAnIntegerValidationMessage : $"{element.Properties.Label} must be a whole number"
                };
            }

            if (value.Length > element.Properties.MaxLength)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"{element.Properties.Label} must be {element.Properties.MaxLength} digits or less"
                };
            }

            if (!string.IsNullOrEmpty(element.Properties.Max) && !string.IsNullOrEmpty(element.Properties.Min))
            {
                var max = int.Parse(element.Properties.Max);
                var min = int.Parse(element.Properties.Min);

                if (output > max || output < min)
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

                if (output > max)
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
                if (output < min)
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