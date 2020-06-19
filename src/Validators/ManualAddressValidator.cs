using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class ManualAddressValidator : IElementValidator
    {

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {

            if (element.Type != EElementType.AddressManual)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            // TODO: Should all these validation messages be editable in the DSL?
            
            var valueAddressLine1 = viewModel.ContainsKey(element.GetCustomItemId(AddressManualConstants.ADDRESS_LINE_1))
                ? viewModel[element.GetCustomItemId(AddressManualConstants.ADDRESS_LINE_1)]
                : null;
            var addressLine1Valid = !string.IsNullOrEmpty(valueAddressLine1);
            var addressLine1Message = addressLine1Valid ? string.Empty : "Please enter address line 1";

            var valueAddressTown = viewModel.ContainsKey(element.GetCustomItemId(AddressManualConstants.TOWN))
                ? viewModel[element.GetCustomItemId(AddressManualConstants.TOWN)]
                : null;
            var addressTownValid = !string.IsNullOrEmpty(valueAddressTown);
            var addressTownMessage = addressTownValid ? string.Empty : "Please enter a town or city";

            var valueAddressPostcode = viewModel.ContainsKey(element.GetCustomItemId(AddressManualConstants.POSTCODE))
                ? viewModel[element.GetCustomItemId(AddressManualConstants.POSTCODE)]
                : null;
            var addressPostcodeMessage = string.Empty;
            var addressPostcodeValid = true;

            if (string.IsNullOrEmpty(valueAddressPostcode))
            {
                addressPostcodeMessage = "Please enter a postcode";
                addressPostcodeValid = false;
            }
            else if (!AddressConstants.STOCKPORT_POSTCODE_REGEX.IsMatch(valueAddressPostcode) && element.Properties.StockportPostcode == true)
            {
                addressPostcodeMessage = "Please enter a valid Stockport postcode";
                addressPostcodeValid = false;
            }
            else if(!AddressConstants.POSTCODE_REGEX.IsMatch(valueAddressPostcode))
            {
                addressPostcodeMessage = "Please enter a valid Postcode";
                addressPostcodeValid = false;
            }

            var isValid = addressLine1Valid && addressTownValid && addressPostcodeValid;

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : $"{addressLine1Message}, {addressTownMessage}, {addressPostcodeMessage}"
            };
        }
    }
}