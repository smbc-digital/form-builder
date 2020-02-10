using form_builder.Enum;
using form_builder.Models.Elements;
using System;
using System.Collections.Generic;

namespace form_builder.Validators
{
    public class DatePickerElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {
            if (element.Type != EElementType.DatePicker || !viewModel.ContainsKey(element.Properties.QuestionId))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var date = viewModel[element.Properties.QuestionId];

            var isValid = !string.IsNullOrEmpty(date);

            if (!isValid && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (!isValid && !element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Check the date and try again"
                };
            }

            var isValidDate = DateTime.TryParse(date, out DateTime  dateValue);
            var todaysDate = DateTime.Now;
            var maxDate = string.IsNullOrEmpty(element.Properties.Max) ? todaysDate.AddYears(100) : new DateTime(int.Parse(element.Properties.Max), todaysDate.Month, todaysDate.Day);

            if(dateValue > maxDate)
            {
                return new ValidationResult {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.UpperLimitValidationMessage) ? element.Properties.UpperLimitValidationMessage : $"Year must be less than or equal to {maxDate.Year}"
                };
            }

            return new ValidationResult
            {
                IsValid = isValidDate,
                Message = isValidDate ? string.Empty : !string.IsNullOrEmpty(element.Properties.ValidationMessageInvalidDate) ? element.Properties.ValidationMessageInvalidDate : "Check the date and try again"
            };
        }
    }
}