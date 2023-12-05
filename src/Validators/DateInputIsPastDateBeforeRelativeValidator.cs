using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.RelativeDateHelper;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class DateInputIsPastDateBeforeRelativeValidator : IElementValidator
    {
        private IRelativeDateHelper _relativeDateHelper;

        public DateInputIsPastDateBeforeRelativeValidator(IRelativeDateHelper relativeDateHelper) => _relativeDateHelper = relativeDateHelper;

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.DateInput) || string.IsNullOrEmpty(element.Properties.IsPastDateBeforeRelative))
                return new ValidationResult { IsValid = true };

            if (_relativeDateHelper.HasValidDate(element, viewModel))
            {
                var relativeDate = _relativeDateHelper.GetRelativeDate(element.Properties.IsPastDateBeforeRelative);
                var maximumDate = DateTime.Today;

                if (relativeDate.Unit.Equals(DateInputConstants.YEAR))
                    maximumDate = DateTime.Today.AddYears(-relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.MONTH))
                    maximumDate = DateTime.Today.AddMonths(-relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.DAY))
                    maximumDate = DateTime.Today.AddDays(-relativeDate.Ammount);

                var chosenDate = _relativeDateHelper.GetChosenDate(element, viewModel);

                if (relativeDate.Type.Equals(DateInputConstants.INCLUSIVE) && maximumDate < _relativeDateHelper.GetChosenDate(element, viewModel) ||
                    relativeDate.Type.Equals(DateInputConstants.EXCLUSIVE) && maximumDate <= _relativeDateHelper.GetChosenDate(element, viewModel))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageIsPastDateBeforeRelative) ? element.Properties.ValidationMessageIsPastDateBeforeRelative : "Check the date and try again"
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
