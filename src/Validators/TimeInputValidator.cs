using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models.Elements;
using System.Collections.Generic;

namespace form_builder.Validators
{ 
    public class TimeInputValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if (element.Type != EElementType.TimeInput)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var valueHours = viewModel.ContainsKey($"{element.Properties.QuestionId}{TimeConstants.HOURS_SUFFIX}")
                ? viewModel[$"{element.Properties.QuestionId}{TimeConstants.HOURS_SUFFIX}"]
                : null;

            var valueMinutes = viewModel.ContainsKey($"{element.Properties.QuestionId}{TimeConstants.MINUTES_SUFFIX}")
                ? viewModel[$"{element.Properties.QuestionId}{TimeConstants.MINUTES_SUFFIX}"]
                : null;

            var valueAmPm = viewModel.ContainsKey($"{element.Properties.QuestionId}{TimeConstants.AM_PM_SUFFIX}")
                ? viewModel[$"{element.Properties.QuestionId}{TimeConstants.AM_PM_SUFFIX}"]
                : null;


            if (string.IsNullOrEmpty(valueHours) && string.IsNullOrEmpty(valueMinutes) && string.IsNullOrEmpty(valueAmPm) && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var isValid = !string.IsNullOrEmpty(valueHours) && !string.IsNullOrEmpty(valueMinutes);

            if (!isValid)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Check the time and try again"
                };
            }

            var isSelected = !string.IsNullOrEmpty(valueAmPm);
            if (!isSelected)
            {
                return new ValidationResult
                {
                    IsValid = isSelected,
                    Message = isSelected ? string.Empty : !string.IsNullOrEmpty(element.Properties.CustomValidationMessageAmPm) ? element.Properties.CustomValidationMessageAmPm : "Choose AM or PM"
                };
            }

            int.TryParse(valueHours, out int hours);
            int.TryParse(valueMinutes, out int minutes);

            var isValidTime = (hours < 13 && hours > 0) && (minutes < 60 && minutes >= 0);

            return new ValidationResult
            {
                IsValid = isValidTime,
                Message = isValidTime ? string.Empty : !string.IsNullOrEmpty(element.Properties.ValidationMessageInvalidTime) ? element.Properties.ValidationMessageInvalidTime : "Check the time and try again"
            };
        }
    }
}
