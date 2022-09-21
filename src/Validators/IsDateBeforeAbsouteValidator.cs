using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class IsDateBeforeAbsoluteValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if ((!element.Type.Equals(EElementType.DatePicker) &&
                !element.Type.Equals(EElementType.DateInput)) ||
                string.IsNullOrEmpty(element.Properties.IsDateBeforeAbsolute))
            {
                return new ValidationResult { IsValid = true };
            }

            DateTime? dateValue = new DateTime();
            if (element.Type.Equals(EElementType.DatePicker))
                dateValue = DatePicker.GetDate(viewModel, element.Properties.QuestionId);

            if (element.Type.Equals(EElementType.DateInput))
                dateValue = DateInput.GetDate(viewModel, element.Properties.QuestionId);

            if (!dateValue.HasValue)
                return new ValidationResult { IsValid = true };

            if (!DateTime.TryParse(element.Properties.IsDateBeforeAbsolute, out DateTime comparisonDateValue))
                throw new FormatException("IsDateBeforeAbsoluteValidator: The comparison date format was incorrect");

            if (dateValue < comparisonDateValue)
                return new ValidationResult { IsValid = true };

            return new ValidationResult
            {
                IsValid = false,
                Message = !string.IsNullOrEmpty(element.Properties.IsDateBeforeValidationMessage)
                    ? element.Properties.IsDateBeforeValidationMessage
                    : string.Format(ValidationConstants.IS_DATE_BEFORE_VALIDATOR_DEFAULT, element.Properties.IsDateBeforeAbsolute)
            };
        }
    }
}