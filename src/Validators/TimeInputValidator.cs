namespace form_builder.Validators;

public class TimeInputValidator : IElementValidator
{
    public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
    {
        if (!element.Type.Equals(EElementType.TimeInput))
            return new ValidationResult { IsValid = true };

        var valueHours = viewModel.ContainsKey($"{element.Properties.QuestionId}{TimeConstants.HOURS_SUFFIX}") ? viewModel[$"{element.Properties.QuestionId}{TimeConstants.HOURS_SUFFIX}"] : null;
        var valueMinutes = viewModel.ContainsKey($"{element.Properties.QuestionId}{TimeConstants.MINUTES_SUFFIX}") ? viewModel[$"{element.Properties.QuestionId}{TimeConstants.MINUTES_SUFFIX}"] : null;
        var valueAmPm = viewModel.ContainsKey($"{element.Properties.QuestionId}{TimeConstants.AM_PM_SUFFIX}") ? viewModel[$"{element.Properties.QuestionId}{TimeConstants.AM_PM_SUFFIX}"] : null;

        if (string.IsNullOrEmpty(valueHours) && string.IsNullOrEmpty(valueMinutes) && string.IsNullOrEmpty(valueAmPm) && element.Properties.Optional)
            return new ValidationResult { IsValid = true };

        var hasTimeValues = !string.IsNullOrEmpty(valueHours) && !string.IsNullOrEmpty(valueMinutes);

        if (!hasTimeValues)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage)
                    ? element.Properties.CustomValidationMessage
                    : "Check the time and try again"
            };
        }

        if (string.IsNullOrEmpty(valueAmPm))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessageAmPm)
                    ? element.Properties.CustomValidationMessageAmPm
                    : "Choose AM or PM"
            };
        }

        int.TryParse(valueHours, out int hours);
        int.TryParse(valueMinutes, out int minutes);

        var isValidTime = hours >= 1 && hours <= 12 && minutes >= 0 && minutes <= 59;

        if (!isValidTime)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageInvalidTime)
                    ? element.Properties.ValidationMessageInvalidTime
                    : "Check the time and try again"
            };
        }

        var enteredTime = DateTime.Parse($"{hours}:{minutes:D2} {valueAmPm}").TimeOfDay;
        var isBeforeMinTime = !string.IsNullOrEmpty(element.Properties.MinTime) && enteredTime < TimeSpan.Parse(element.Properties.MinTime);
        var isAfterMaxTime = !string.IsNullOrEmpty(element.Properties.MaxTime) && enteredTime > TimeSpan.Parse(element.Properties.MaxTime);
        var isWithinTimeRange = !isBeforeMinTime && !isAfterMaxTime;

        return new ValidationResult
        {
            IsValid = isWithinTimeRange,
            Message = isWithinTimeRange
                ? string.Empty
                : !string.IsNullOrEmpty(element.Properties.ValidationMessageInvalidTime)
                    ? element.Properties.ValidationMessageInvalidTime
                    : "Check the time and try again"
        };
    }
}