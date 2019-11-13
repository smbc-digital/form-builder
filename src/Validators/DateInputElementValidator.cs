using form_builder.Enum;
using form_builder.Models;
using System;
using System.Collections.Generic;

namespace form_builder.Validators
{
    public class DateInputElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {
            if (element.Type != EElementType.DateInput)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if ((element.Properties.Optional.HasValue && element.Properties.Optional.Value))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var valueDay = viewModel.ContainsKey($"{element.Properties.QuestionId}-day")
                ? viewModel[$"{element.Properties.QuestionId}-day"]
                : null;

            var valueMonth = viewModel.ContainsKey($"{element.Properties.QuestionId}-month")
                ? viewModel[$"{element.Properties.QuestionId}-month"]
                : null;

            var valueYear = viewModel.ContainsKey($"{element.Properties.QuestionId}-year")
                ? viewModel[$"{element.Properties.QuestionId}-year"]
                : null;

            var isValid = !string.IsNullOrEmpty(valueDay) || !string.IsNullOrEmpty(valueMonth) || !string.IsNullOrEmpty(valueYear);

            DateTime dateValue;
            var dateOutput = new DateTime();
            if (isValid && !DateTime.TryParse($"{valueDay}/{valueMonth}/{valueYear}", out dateValue))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"Invalid date input"
                };
            }
            else
            {
                dateOutput = DateTime.Parse($"{valueDay}/{valueMonth}/{valueYear}");
            }

            if(isValid && element.Properties.RestrictCurrentDate && dateOutput.Date == DateTime.Today)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"{element.Properties.Label} today not allowed"
                };
            }

            if(isValid && element.Properties.RestrictFutureDate && dateOutput > DateTime.Now)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    //Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageRestrictFutureDate) ? element.Properties.ValidationMessageRestrictFutureDate : $"{element.Properties.Label} date in future not allowed"
                    Message = element.Properties.ValidationMessageRestrictFutureDate
                };
            }

            if (isValid && element.Properties.RestrictPastDate && dateOutput < DateTime.Now)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"{element.Properties.Label} date in past not allowed"
                };
            }

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : $"{element.Properties.Label} is required"
            };
        }
    }
}
