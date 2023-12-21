using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.RelativeDateHelper;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class DateInputIsPastDateAfterRelativeValidator : IElementValidator
    {
        private IRelativeDateHelper _relativeDateHelper;

        public DateInputIsPastDateAfterRelativeValidator(IRelativeDateHelper relativeDateHelper) => _relativeDateHelper = relativeDateHelper;

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.DateInput) || string.IsNullOrEmpty(element.Properties.IsPastDateAfterRelative))
                return new ValidationResult { IsValid = true };

            if (_relativeDateHelper.HasValidDate(element, viewModel))
            {
                var relativeDate = _relativeDateHelper.GetRelativeDate(element.Properties.IsPastDateAfterRelative);
                var maximumDate = DateTime.Today;

                if (relativeDate.Unit.Equals(DateInputConstants.YEAR))
                    maximumDate = DateTime.Today.AddYears(-relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.MONTH))
                    maximumDate = DateTime.Today.AddMonths(-relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.DAY))
                    maximumDate = DateTime.Today.AddDays(-relativeDate.Ammount);

                var chosenDate = _relativeDateHelper.GetChosenDate(element, viewModel);

                if (relativeDate.Type.Equals(DateInputConstants.INCLUSIVE) && maximumDate > _relativeDateHelper.GetChosenDate(element, viewModel) ||
                    relativeDate.Type.Equals(DateInputConstants.EXCLUSIVE) && maximumDate >= _relativeDateHelper.GetChosenDate(element, viewModel))
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
}
