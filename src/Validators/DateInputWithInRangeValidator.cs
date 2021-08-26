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

            var inputDate = DateTime.Now;
            var isValidDate = DateTime.TryParse($"{valueDay}/{valueMonth}/{valueYear}", out inputDate);

            var dateType = element.Properties.WithinRange.Substring(element.Properties.WithinRange.LastIndexOf('-') + 1).Trim();
            string value = element.Properties.WithinRange.Split('-')[0].Trim();
            var valueToSubtract = Convert.ToInt32(value);

            var date = DateTime.Today; //make sure it fix in one condition
            if (dateType.Equals("Y"))
                date = DateTime.Today.AddYears(-valueToSubtract);

            if (dateType.Equals("M"))
                date = DateTime.Today.AddMonths(-valueToSubtract);

            if (dateType.Equals("D"))
                date = DateTime.Today.AddDays(-valueToSubtract);

            if (!(inputDate >= date && inputDate < DateTime.Now))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Check the date and try again"
                };
            }

            return new ValidationResult
            {
                IsValid = true,
                Message = string.Empty
            };
        }
    }
}
