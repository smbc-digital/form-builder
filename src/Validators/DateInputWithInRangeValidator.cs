using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using System;
using System.Collections.Generic;

namespace form_builder.Validators
{
    public class DateInputWithInRangeValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.DateInput) || string.IsNullOrEmpty(element.Properties.WithinRange))
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

            if (!string.IsNullOrEmpty(valueDay) && !string.IsNullOrEmpty(valueMonth) && !string.IsNullOrEmpty(valueYear))
            {
                var inputDate = DateTime.Now;
                var isValidDate = DateTime.TryParse($"{valueDay}/{valueMonth}/{valueYear}", out inputDate);

                if (isValidDate)
                {
                    string value = element.Properties.WithinRange.Split('-')[0].Trim();
                    var valueToSubtract = Convert.ToInt32(value);

                    var date = DateTime.Today;
                    if (element.Properties.WithinRangeType.Equals(DateInputConstants.YEAR))
                        date = DateTime.Today.AddYears(-valueToSubtract);

                    if (element.Properties.WithinRangeType.Equals(DateInputConstants.MONTH))
                        date = DateTime.Today.AddMonths(-valueToSubtract);

                    if (element.Properties.WithinRangeType.Equals(DateInputConstants.DAY))
                        date = DateTime.Today.AddDays(-valueToSubtract);

                    if (!(inputDate >= date && inputDate < DateTime.Now) || (inputDate == date))
                    {
                        return new ValidationResult
                        {
                            IsValid = false,
                            Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Check the date and try again"
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
