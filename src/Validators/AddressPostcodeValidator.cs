using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class AddressPostcodeValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if (element.Type != Enum.EElementType.Address)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var addressElement = (Address)element;

            if (!viewModel.ContainsKey(addressElement.AddressSearchQuestionId))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (string.IsNullOrEmpty(viewModel[addressElement.AddressSearchQuestionId]) && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var value = viewModel[addressElement.AddressSearchQuestionId];
            var isValid = true;
            if (!AddressConstants.POSTCODE_REGEX.Match(value).Success)
            {
                isValid = false;
            }

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : $"{ element.Properties.PostcodeLabel} must be a valid postcode"
            };
        }
    }
}
