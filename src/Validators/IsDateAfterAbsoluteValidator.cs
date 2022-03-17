using System;
using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class IsDateAfterAbsoluteValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            ValidationResult successResult = new() { IsValid = true };

            if ((!element.Type.Equals(EElementType.DatePicker) &&
                !element.Type.Equals(EElementType.DateInput)) ||
                string.IsNullOrEmpty(element.Properties.IsDateAfterAbsolute))
            {
                return successResult;
            }

            DateTime? dateValue = new DateTime();
            if (element.Type.Equals(EElementType.DatePicker))
                dateValue = DatePicker.GetDate(viewModel, element.Properties.QuestionId);

            if (element.Type.Equals(EElementType.DateInput))
                dateValue = DateInput.GetDate(viewModel, element.Properties.QuestionId);

            if (!dateValue.HasValue)
                return new ValidationResult { IsValid = true };

            if (!DateTime.TryParse(element.Properties.IsDateAfterAbsolute, out DateTime comparisonDateValue))
                throw new FormatException("IsDateAfterAbsoluteValidator: The comparison date format was incorrect");

            if (dateValue > comparisonDateValue)
                return successResult;

            return new ValidationResult
            {
                IsValid = false,
                Message = !string.IsNullOrEmpty(element.Properties.IsDateAfterValidationMessage)
                    ? element.Properties.IsDateAfterValidationMessage
                    : string.Format(ValidationConstants.IS_DATE_AFTER_VALIDATOR_DEFAULT, element.Properties.IsDateAfterAbsolute)
            };
        }
    }
}