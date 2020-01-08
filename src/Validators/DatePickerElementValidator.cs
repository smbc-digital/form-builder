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
            if (element.Type != EElementType.DatePicker || element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var date = viewModel.ContainsKey(element.Properties.QuestionId) ? viewModel[element.Properties.QuestionId] : string.Empty;

            var isValid = !string.IsNullOrEmpty(date);

            if (!isValid)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Check the date and try again"
                };
            }

            var isValidDate = DateTime.TryParse(date, out _);

            return new ValidationResult
            {
                IsValid = isValidDate,
                Message = isValidDate ? string.Empty : !string.IsNullOrEmpty(element.Properties.ValidationMessageInvalidDate) ? element.Properties.ValidationMessageInvalidDate : "Check the date and try again"
            };
        }
    }
}