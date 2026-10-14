namespace form_builder.Validators;

public class DateInputIsFutureDateBeforeRelativeValidator(IRelativeDateHelper relativeDateHelper) : IElementValidator
{
    public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
    {
        if (!element.Type.Equals(EElementType.DateInput) || string.IsNullOrEmpty(element.Properties.IsFutureDateBeforeRelative))
            return new ValidationResult { IsValid = true };

        if (relativeDateHelper.HasValidDate(element, viewModel))
        {

            var relativeDate = relativeDateHelper.GetRelativeDate(element.Properties.IsFutureDateBeforeRelative);
            var maximumDate = DateTime.Today;

            if (relativeDate.Unit.Equals(DateInputConstants.YEAR))
                maximumDate = DateTime.Today.AddYears(relativeDate.Amount);

            if (relativeDate.Unit.Equals(DateInputConstants.MONTH))
                maximumDate = DateTime.Today.AddMonths(relativeDate.Amount);

            if (relativeDate.Unit.Equals(DateInputConstants.DAY))
                maximumDate = DateTime.Today.AddDays(relativeDate.Amount);

            if (relativeDate.Type.Equals(DateInputConstants.INCLUSIVE) && maximumDate < relativeDateHelper.GetChosenDate(element, viewModel) ||
                relativeDate.Type.Equals(DateInputConstants.EXCLUSIVE) && maximumDate <= relativeDateHelper.GetChosenDate(element, viewModel))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageIsFutureDateBeforeRelative) ? element.Properties.ValidationMessageIsFutureDateBeforeRelative : "Check the date and try again"
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