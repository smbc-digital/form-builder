using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class ManualAddressValidator : IElementValidator
    {

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {

            if (!(element.Type == EElementType.Address && viewModel.IsManual()))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var valueAddressLine1 = viewModel.ContainsKey($"{element.Properties.QuestionId}-{AddressManualConstants.ADDRESS_LINE_1}")
                ? viewModel[$"{element.Properties.QuestionId}-{AddressManualConstants.ADDRESS_LINE_1}"]
                : null;

            var addressLine1Valid = !string.IsNullOrEmpty(valueAddressLine1);
            var addressLine1Message = addressLine1Valid ? string.Empty : "Enter the address";

            var valueAddressTown = viewModel.ContainsKey($"{element.Properties.QuestionId}-{AddressManualConstants.TOWN}")
                ? viewModel[$"{element.Properties.QuestionId}-{AddressManualConstants.TOWN}"]
                : null;

            var addressTownValid = !string.IsNullOrEmpty(valueAddressTown);
            var addressTownMessage = addressTownValid ? string.Empty : "Enter the town or city";

            var valueAddressPostcode = viewModel.ContainsKey($"{element.Properties.QuestionId}-{AddressManualConstants.POSTCODE}")
                ? viewModel[$"{element.Properties.QuestionId}-{AddressManualConstants.POSTCODE}"]
                : null;

            var addressPostcodeMessage = string.Empty;
            var addressPostcodeValid = true;


            if (string.IsNullOrEmpty(valueAddressPostcode))
            {
                addressPostcodeMessage = "Enter a postcode";
                addressPostcodeValid = false;
            }
            else if (!AddressConstants.STOCKPORT_POSTCODE_REGEX.IsMatch(valueAddressPostcode) && element.Properties.StockportPostcode == true)
            {
                addressPostcodeMessage = "Enter a valid Stockport postcode";
                addressPostcodeValid = false;
            }
            else if (!AddressConstants.POSTCODE_REGEX.IsMatch(valueAddressPostcode))
            {
                addressPostcodeMessage = "Enter a valid Postcode";
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