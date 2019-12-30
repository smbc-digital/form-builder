using form_builder.Enum;
using form_builder.Models.Elements;
using System;
using System.Collections.Generic;

namespace form_builder.Validators
{ 
    public class TimeInputValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {
            if (element.Type != EElementType.TimeInput)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var valueHours = viewModel.ContainsKey($"{element.Properties.QuestionId}-hours")
                ? viewModel[$"{element.Properties.QuestionId}-hours"]
                : null;

            var valueMinutes = viewModel.ContainsKey($"{element.Properties.QuestionId}-minutes")
                ? viewModel[$"{element.Properties.QuestionId}-minutes"]
                : null;

            var valueAm = viewModel.ContainsKey($"{element.Properties.QuestionId}-am")
                ? viewModel[$"{element.Properties.QuestionId}-am"]
                : null;

            var valuePm = viewModel.ContainsKey($"{element.Properties.QuestionId}-pm")
                ? viewModel[$"{element.Properties.QuestionId}-pm"]
                : null;

            if (string.IsNullOrEmpty(valueHours) && string.IsNullOrEmpty(valueMinutes) && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var isValid = !string.IsNullOrEmpty(valueHours) || !string.IsNullOrEmpty(valueMinutes);

            if (!isValid)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Check the time and try again"
                };
            }

            var isSelected = (!string.IsNullOrEmpty(valueAm) || !string.IsNullOrEmpty(valuePm));
            if (!isSelected)
            {
                return new ValidationResult
                {
                    IsValid = isSelected,
                    Message = isSelected ? string.Empty : !string.IsNullOrEmpty(element.Properties.CustomValidationMessageAmPm) ? element.Properties.CustomValidationMessageAmPm : "Choose AM or PM"
                };
            }

            var isValidDate = DateTime.TryParse($"{valueHours}:{valueMinutes}{valueAm}{valuePm}", out _);

            return new ValidationResult
            {
                IsValid = isValidDate,
                Message = isValidDate ? string.Empty : !string.IsNullOrEmpty(element.Properties.ValidationMessageInvalidTime) ? element.Properties.ValidationMessageInvalidTime : "Check the time and try again"
            };
        }

    }
}
