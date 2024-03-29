﻿using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class RestrictPastDatepickerValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Properties.RestrictPastDate || !element.Type.Equals(EElementType.DatePicker) || element.Properties.Optional)
                return new ValidationResult { IsValid = true };

            var value = viewModel.ContainsKey(element.Properties.QuestionId) ? viewModel[element.Properties.QuestionId] : null;
            var outDate = DateTime.Now;
            var isValidDate = DateTime.TryParse(value, out outDate);

            if (!isValidDate)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage)
                        ? element.Properties.CustomValidationMessage
                        : "Check the date and try again"
                };
            }

            var date = DateTime.Today;
            var dateOutput = DateTime.Parse(value);

            if (dateOutput < date && element.Properties.RestrictPastDate)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageRestrictPastDate)
                        ? element.Properties.ValidationMessageRestrictPastDate
                        : "Check the date and try again"
                };
            }

            return new ValidationResult
            {
                IsValid = true,
                Message = string.Empty
            };
        }
    }
}