using System;
using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class DatePickerElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
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
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : ValidationConstants.DatePickerDefault
                };
            }

            var isValidDate = DateTime.TryParse(date, out DateTime dateValue);
            var todaysDate = DateTime.Now;
            var maxDate = string.IsNullOrEmpty(element.Properties.Max) ? todaysDate.AddYears(100) : new DateTime(int.Parse(element.Properties.Max), todaysDate.Month, todaysDate.Day);

            if (dateValue > maxDate)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.UpperLimitValidationMessage) ? element.Properties.UpperLimitValidationMessage : ValidationConstants.DatePickerDefault
                };
            }

            return new ValidationResult
            {
                IsValid = isValidDate,
                Message = isValidDate ? string.Empty : !string.IsNullOrEmpty(element.Properties.ValidationMessageInvalidDate) ? element.Properties.ValidationMessageInvalidDate : ValidationConstants.DatePickerDefault
            };
        }
    }
}