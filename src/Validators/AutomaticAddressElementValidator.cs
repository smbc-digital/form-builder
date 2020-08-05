using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class    AutomaticAddressElementValidator : IElementValidator
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
            if (!viewModel.ContainsKey(addressElement.AddressSelectQuestionId))
            {
                return new ValidationResult{
                    IsValid = true
                };
            }

            var value = viewModel[addressElement.AddressSelectQuestionId];
            if (addressElement.Properties.Optional && string.IsNullOrEmpty(value))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var isValid = AddressConstants.UPRN_REGEX.IsMatch(value); 

            return new ValidationResult{
                IsValid = isValid,
                Message = isValid ? string.Empty : $"please select an address"
            }; 
        }
    }
}