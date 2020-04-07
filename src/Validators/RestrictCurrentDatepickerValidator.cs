using System;
using System.Collections.Generic;
using System.Globalization;
using form_builder.Models.Elements;
using form_builder.Enum;


namespace form_builder.Validators
{
    public class RestrictCurrentDatepickerValidator : IElementValidator
    {
        private readonly CultureInfo _culture_info = new CultureInfo("en-gb");

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if (!element.Properties.RestrictCurrentDate || element.Type != EElementType.DatePicker || element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var value = viewModel.ContainsKey(element.Properties.QuestionId) ? viewModel[element.Properties.QuestionId] : null;

            var outDate = DateTime.Now;

            var isValidDate = DateTime.TryParse(value,out outDate); ;

            if (!isValidDate)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Check the date and try again"
                };
            }

            var date = DateTime.Today;

            var dateOutput = DateTime.Parse(value);

            if (dateOutput == date)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageRestrictFutureDate) ? element.Properties.ValidationMessageRestrictFutureDate : "Check the date and try again"
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
