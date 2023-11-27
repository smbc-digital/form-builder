using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.RelativeDateHelper;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class DateInputIsPastDateBeforeRelativeValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.DateInput) || string.IsNullOrEmpty(element.Properties.IsPastDateBeforeRelative))
                return new ValidationResult { IsValid = true };

            var relativeDateHelper = new RelativeDateHelper(element, viewModel);

            if (relativeDateHelper.HasValidDate())
            {
                var relativeDate = relativeDateHelper.GetRelativeDate(element.Properties.IsPastDateBeforeRelative);
                var maximumDate = DateTime.Today;

                if (relativeDate.Unit.Equals(DateInputConstants.YEAR))
                    maximumDate = DateTime.Today.AddYears(-relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.MONTH))
                    maximumDate = DateTime.Today.AddMonths(-relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.DAY))
                    maximumDate = DateTime.Today.AddDays(-relativeDate.Ammount);

                if (relativeDate.Type.Equals(DateInputConstants.INCLUISIVE) && maximumDate < relativeDateHelper.ChosenDate() ||
                    relativeDate.Type.Equals(DateInputConstants.EXCLUSIVE) && maximumDate <= relativeDateHelper.ChosenDate())
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
