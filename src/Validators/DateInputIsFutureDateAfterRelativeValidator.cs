using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.RelativeDateHelper;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class DateInputIsFutureDateAfterRelativeValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.DateInput) || string.IsNullOrEmpty(element.Properties.IsFutureDateAfterRelative))
                return new ValidationResult { IsValid = true };

            var relativeDateHelper = new RelativeDateHelper(element, viewModel);

            if (relativeDateHelper.HasValidDate())
            {
                var relativeDate = relativeDateHelper.GetRelativeDate(element.Properties.IsFutureDateAfterRelative);
                var minimumDate = DateTime.Today;

                if (relativeDate.Unit.Equals(DateInputConstants.YEAR))
                    minimumDate = DateTime.Today.AddYears(relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.MONTH))
                    minimumDate = DateTime.Today.AddMonths(relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.DAY))
                    minimumDate = DateTime.Today.AddDays(relativeDate.Ammount);

                if (relativeDate.Type.Equals(DateInputConstants.INCLUISIVE) && minimumDate > relativeDateHelper.ChosenDate() ||
                    relativeDate.Type.Equals(DateInputConstants.EXCLUSIVE) && minimumDate >= relativeDateHelper.ChosenDate())
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
}
