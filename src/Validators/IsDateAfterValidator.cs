using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers;

namespace form_builder.Validators
{
    public class IsDateAfterValidator : IElementValidator
    {
        private readonly IFormAnswersProvider _formAnswersProvider;

        public IsDateAfterValidator(IFormAnswersProvider formAnswersProvider) =>
            _formAnswersProvider = formAnswersProvider;

        public ValidationResult Validate(Element currentElement, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            IElement comparisonElement = baseForm.GetElement(currentElement.Properties.IsDateAfter);

            if (!IsValidatorRelevant(currentElement, comparisonElement))
                return new ValidationResult { IsValid = true };

            DateTime? currentElementValue = GetElementValue(currentElement, viewModel);
            if (!currentElementValue.HasValue)
                return new ValidationResult { IsValid = true };

            DateTime? comparisonElementValue = GetElementValue(comparisonElement, viewModel);
            if (!comparisonElementValue.HasValue)
            {
                FormAnswers answers = _formAnswersProvider.GetFormAnswers(baseForm.FormName);
                comparisonElementValue = GetElementValue(comparisonElement, answers);
            }

            if (!comparisonElementValue.HasValue)
                return new ValidationResult { IsValid = true };

            if (currentElementValue > comparisonElementValue)
                return new ValidationResult { IsValid = true };

            if (currentElement.Properties.IsDateEqualityAllowed && currentElementValue.Equals(comparisonElementValue))
                return new ValidationResult { IsValid = true };

            return new ValidationResult
            {
                IsValid = false,
                Message = !string.IsNullOrEmpty(currentElement.Properties.IsDateAfterValidationMessage)
                    ? currentElement.Properties.IsDateAfterValidationMessage
                    : string.Format(ValidationConstants.IS_DATE_AFTER_VALIDATOR_DEFAULT, currentElement.Properties.IsDateAfter)
            };
        }

        private bool IsValidatorRelevant(IElement element, IElement comparisonElement)
        {
            if (!element.Type.Equals(EElementType.DatePicker) && !element.Type.Equals(EElementType.DateInput))
                return false;

            if (comparisonElement is null)
                return false;

            if (!comparisonElement.Type.Equals(EElementType.DatePicker) && !comparisonElement.Type.Equals(EElementType.DateInput))
                return false;

            if ((string.IsNullOrEmpty(element.Properties.IsDateAfter)))
                return false;

            return true;
        }

        private DateTime? GetElementValue(IElement element, Dictionary<string, dynamic> viewModel)
        {
            if (element.Type.Equals(EElementType.DatePicker))
                return DatePicker.GetDate(viewModel, element.Properties.QuestionId);

            if (element.Type.Equals(EElementType.DateInput))
                return DateInput.GetDate(viewModel, element.Properties.QuestionId);

            return new DateTime();
        }

        private DateTime? GetElementValue(IElement element, FormAnswers formAnswers)
        {
            if (element.Type.Equals(EElementType.DatePicker))
                return DatePicker.GetDate(formAnswers, element.Properties.QuestionId);

            if (element.Type.Equals(EElementType.DateInput))
                return DateInput.GetDate(formAnswers, element.Properties.QuestionId);

            return new DateTime();
        }
    }
}