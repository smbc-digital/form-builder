namespace form_builder.Validators;

public class DateInputIsPastDateAfterRelativeValidator(IRelativeDateHelper relativeDateHelper) : IElementValidator
{
    public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
    {
        if (!element.Type.Equals(EElementType.DateInput) || string.IsNullOrEmpty(element.Properties.IsPastDateAfterRelative))
            return new ValidationResult { IsValid = true };

        if (relativeDateHelper.HasValidDate(element, viewModel))
        {
            var relativeDate = relativeDateHelper.GetRelativeDate(element.Properties.IsPastDateAfterRelative);
            var maximumDate = DateTime.Today;

            if (relativeDate.Unit.Equals(DateInputConstants.YEAR))
                maximumDate = DateTime.Today.AddYears(-relativeDate.Amount);

            if (relativeDate.Unit.Equals(DateInputConstants.MONTH))
                maximumDate = DateTime.Today.AddMonths(-relativeDate.Amount);

            if (relativeDate.Unit.Equals(DateInputConstants.DAY))
                maximumDate = DateTime.Today.AddDays(-relativeDate.Amount);

            var chosenDate = relativeDateHelper.GetChosenDate(element, viewModel);

            if (relativeDate.Type.Equals(DateInputConstants.INCLUSIVE) && maximumDate > chosenDate ||
                relativeDate.Type.Equals(DateInputConstants.EXCLUSIVE) && maximumDate >= chosenDate)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageIsPastDateAfterRelative) ? element.Properties.ValidationMessageIsPastDateAfterRelative : "Check the date and try again"
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