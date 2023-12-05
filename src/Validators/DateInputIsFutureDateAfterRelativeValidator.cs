using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.RelativeDateHelper;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class DateInputIsFutureDateAfterRelativeValidator : IElementValidator
    {
        private IRelativeDateHelper _relativeDateHelper;

        public DateInputIsFutureDateAfterRelativeValidator(IRelativeDateHelper relativeDateHelper) => _relativeDateHelper = relativeDateHelper;

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.DateInput) || string.IsNullOrEmpty(element.Properties.IsFutureDateAfterRelative))
                return new ValidationResult { IsValid = true };

            if (_relativeDateHelper.HasValidDate(element, viewModel))
            {
                var relativeDate = _relativeDateHelper.GetRelativeDate(element.Properties.IsFutureDateAfterRelative);
                var minimumDate = DateTime.Today;

                if (relativeDate.Unit.Equals(DateInputConstants.YEAR))
                    minimumDate = DateTime.Today.AddYears(relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.MONTH))
                    minimumDate = DateTime.Today.AddMonths(relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.DAY))
                    minimumDate = DateTime.Today.AddDays(relativeDate.Ammount);

                if (relativeDate.Type.Equals(DateInputConstants.INCLUSIVE) && minimumDate > _relativeDateHelper.GetChosenDate(element, viewModel) ||
                    relativeDate.Type.Equals(DateInputConstants.EXCLUSIVE) && minimumDate >= _relativeDateHelper.GetChosenDate(element, viewModel))
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
