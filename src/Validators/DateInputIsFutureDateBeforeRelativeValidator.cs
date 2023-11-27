using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.RelativeDateHelper;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class DateInputIsFutureDateBeforeRelativeValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.DateInput) || string.IsNullOrEmpty(element.Properties.IsFutureDateBeforeRelative))
                return new ValidationResult { IsValid = true };

            var relativeDateHelper = new RelativeDateHelper(element, viewModel);

            var valueDay = viewModel.ContainsKey($"{element.Properties.QuestionId}-day")
                ? viewModel[$"{element.Properties.QuestionId}-day"]
                : null;

            var valueMonth = viewModel.ContainsKey($"{element.Properties.QuestionId}-month")
                ? viewModel[$"{element.Properties.QuestionId}-month"]
                : null;

            var valueYear = viewModel.ContainsKey($"{element.Properties.QuestionId}-year")
                ? viewModel[$"{element.Properties.QuestionId}-year"]
                : null;

            if (relativeDateHelper.HasValidDate())
            {

                var relativeDate = relativeDateHelper.GetRelativeDate(element.Properties.IsFutureDateBeforeRelative);
                var maximumDate = DateTime.Today;

                if (relativeDate.Unit.Equals(DateInputConstants.YEAR))
                    maximumDate = DateTime.Today.AddYears(relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.MONTH))
                    maximumDate = DateTime.Today.AddMonths(relativeDate.Ammount);

                if (relativeDate.Unit.Equals(DateInputConstants.DAY))
                    maximumDate = DateTime.Today.AddDays(relativeDate.Ammount);

                if (relativeDate.Type.Equals(DateInputConstants.INCLUISIVE) && maximumDate < relativeDateHelper.ChosenDate() ||
                    relativeDate.Type.Equals(DateInputConstants.EXCLUSIVE) && maximumDate <= relativeDateHelper.ChosenDate())
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
}
