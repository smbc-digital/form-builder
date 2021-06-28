using System;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class DateInputElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.DateInput))
                return new ValidationResult { IsValid = true };

            var valueDay = viewModel.ContainsKey($"{element.Properties.QuestionId}-day")
                ? viewModel[$"{element.Properties.QuestionId}-day"]
                : null;

            var valueMonth = viewModel.ContainsKey($"{element.Properties.QuestionId}-month")
                ? viewModel[$"{element.Properties.QuestionId}-month"]
                : null;

            var valueYear = viewModel.ContainsKey($"{element.Properties.QuestionId}-year")
                ? viewModel[$"{element.Properties.QuestionId}-year"]
                : null;

            var isOptional = string.IsNullOrEmpty(valueDay) && string.IsNullOrEmpty(valueMonth) && string.IsNullOrEmpty(valueYear) && element.Properties.Optional;
            if (isOptional)
                return new ValidationResult { IsValid = true };

            var isValid = !string.IsNullOrEmpty(valueDay) || !string.IsNullOrEmpty(valueMonth) || !string.IsNullOrEmpty(valueYear);
            if (!isValid)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage)
                        ? element.Properties.CustomValidationMessage
                        : "Check the date and try again"
                };
            }

            var isValidDate = DateTime.TryParse($"{valueDay}/{valueMonth}/{valueYear}", out DateTime date);
            if (isValidDate)
            {
                var maxYear = DateTime.Now.Year + 100;
                if (date.Year > maxYear)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = !string.IsNullOrEmpty(element.Properties.UpperLimitValidationMessage)
                            ? element.Properties.UpperLimitValidationMessage
                            : $"Year must be less than or equal to {maxYear}"
                    };
                }
            }

            return new ValidationResult
            {
                IsValid = isValidDate,
                Message = isValidDate
                    ? string.Empty
                    : !string.IsNullOrEmpty(element.Properties.ValidationMessageInvalidDate) ? element.Properties.ValidationMessageInvalidDate : "Check the date and try again"
            };
        }
    }
}