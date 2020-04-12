using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class AutomaticAddressElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if (!viewModel.ContainsKey(element.GetCustomItemId(AddressConstants.ADDRESS_SELECT)))
            {
                return new ValidationResult{
                    IsValid = true
                };
            }

            var value = viewModel[element.GetCustomItemId(AddressConstants.ADDRESS_SELECT)];
            if (element.Properties.Optional && string.IsNullOrEmpty(value))
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