using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class StockportPostcodeElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.Textbox))
                return new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(viewModel[element.Properties.QuestionId]) && element.Properties.Optional)
                return new ValidationResult { IsValid = true };

            if (!element.Properties.StockportPostcode || !viewModel.ContainsKey(element.Properties.QuestionId))
                return new ValidationResult { IsValid = true };

            var value = viewModel[element.Properties.QuestionId];
            var isValid = true;

            if (!AddressConstants.STOCKPORT_POSTCODE_REGEX.Match(value).Success)
                isValid = false;

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : ValidationConstants.POSTCODE_INCORRECT_FORMAT
            };
        }
    }
}