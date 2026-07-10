namespace form_builder.Validators;

public class DateInputIsFutureDateAfterRelativeValidator(IRelativeDateHelper relativeDateHelper) : IElementValidator
{
    public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
    {
        if (!element.Type.Equals(EElementType.DateInput) || string.IsNullOrEmpty(element.Properties.IsFutureDateAfterRelative))
            return new ValidationResult { IsValid = true };

        if (relativeDateHelper.HasValidDate(element, viewModel))
        {
            var relativeDate = relativeDateHelper.GetRelativeDate(element.Properties.IsFutureDateAfterRelative);
            var minimumDate = DateTime.Today;

            if (relativeDate.Unit.Equals(DateInputConstants.YEAR))
                minimumDate = DateTime.Today.AddYears(relativeDate.Amount);

            if (relativeDate.Unit.Equals(DateInputConstants.MONTH))
                minimumDate = DateTime.Today.AddMonths(relativeDate.Amount);

            if (relativeDate.Unit.Equals(DateInputConstants.DAY))
                minimumDate = DateTime.Today.AddDays(relativeDate.Amount);

            if (relativeDate.Type.Equals(DateInputConstants.INCLUSIVE) && minimumDate > relativeDateHelper.GetChosenDate(element, viewModel) ||
                relativeDate.Type.Equals(DateInputConstants.EXCLUSIVE) && minimumDate >= relativeDateHelper.GetChosenDate(element, viewModel))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageIsFutureDateAfterRelative) ? element.Properties.ValidationMessageIsFutureDateAfterRelative : "Check the date and try again"
                };
            }

        }

        return new ValidationResult
        {
            IsValid = true,
            Message = string.Empty
        };
    }
}