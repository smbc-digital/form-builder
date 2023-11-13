using form_builder.Constants;
using form_builder.Enum;
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

            var valueDay = viewModel.ContainsKey($"{element.Properties.QuestionId}-day")
                ? viewModel[$"{element.Properties.QuestionId}-day"]
                : null;

            var valueMonth = viewModel.ContainsKey($"{element.Properties.QuestionId}-month")
                ? viewModel[$"{element.Properties.QuestionId}-month"]
                : null;

            var valueYear = viewModel.ContainsKey($"{element.Properties.QuestionId}-year")
                ? viewModel[$"{element.Properties.QuestionId}-year"]
                : null;

            if (valueDay is not null && valueMonth is not null && valueYear is not null)
            {
                var chosenDate = DateTime.Now;
                var isValidDate = DateTime.TryParse($"{valueDay}/{valueMonth}/{valueYear}", out chosenDate);

                if (isValidDate)
                {
                    string value = element.Properties.IsFutureDateAfterRelative.Split('-')[0].Trim();
                    var numberOfDaysInFuture = Convert.ToInt32(value);

                    var minimumDate = DateTime.Today;
                    if (element.Properties.IsFutureDateAfterRelativeType.Equals(DateInputConstants.YEAR))
                        minimumDate = DateTime.Today.AddYears(numberOfDaysInFuture);

                    if (element.Properties.IsFutureDateAfterRelativeType.Equals(DateInputConstants.MONTH))
                        minimumDate = DateTime.Today.AddMonths(numberOfDaysInFuture);

                    if (element.Properties.IsFutureDateAfterRelativeType.Equals(DateInputConstants.DAY))
                        minimumDate = DateTime.Today.AddDays(numberOfDaysInFuture);

                    if (minimumDate >= chosenDate)
                    {
                        return new ValidationResult
                        {
                            IsValid = false,
                            Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageIsFutureDateAfterRelative) ? element.Properties.ValidationMessageIsFutureDateAfterRelative : "Check the date and try again"
                        };
                    }
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
