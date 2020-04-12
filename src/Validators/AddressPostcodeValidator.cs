using System.Collections.Generic;
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

            if (!viewModel.ContainsKey(element.GetCustomItemId(AddressConstants.POSTCODE)))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (string.IsNullOrEmpty(viewModel[element.GetCustomItemId(AddressConstants.POSTCODE)]) && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var value = viewModel[element.GetCustomItemId(AddressConstants.POSTCODE)];
            var isValid = true;
            if (!AddressConstants.POSTCODE_REGEX.Match(value).Success)
            {
                isValid = false;
            }

            // TODO: Should this validation message be editable in the DSL?
            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : $"{ element.Properties.PostcodeLabel} must be a valid postcode"
            };
        }
    }
}
