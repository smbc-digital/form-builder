using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class ManualAddressValidator : IElementValidator
    {
        private readonly Regex _postCode  = new Regex(@"^([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})$");
        private readonly Regex _stockportPostCode= new Regex(@"^(sK|Sk|SK|sk|M|m)[0-9][0-9A-Za-z]?\s?[0-9][A-Za-z]{2}");

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if (!(element.Type == EElementType.Address && viewModel.IsManual()))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var valueAddressLine1 = viewModel.ContainsKey($"{element.Properties.QuestionId}-AddressManualAddressLine1")
                 ? viewModel[$"{element.Properties.QuestionId}-AddressManualAddressLine1"]
                 : null;

            var addressLine1Valid = !string.IsNullOrEmpty(valueAddressLine1);

            var addressLine1Message = addressLine1Valid ? string.Empty : "Please enter Address Line 1";

            var valueAddressTown = viewModel.ContainsKey($"{element.Properties.QuestionId}-AddressManualAddressTown")
                ? viewModel[$"{element.Properties.QuestionId}-AddressManualAddressTown"]
                : null;

            var addressTownValid = !string.IsNullOrEmpty(valueAddressTown);
            var addressTownMessage = addressTownValid ? string.Empty : "Please enter Town";

            var valueAddressPostcode = viewModel.ContainsKey($"{element.Properties.QuestionId}-AddressManualAddressPostcode")
                ? viewModel[$"{element.Properties.QuestionId}-AddressManualAddressPostcode"]
                : null;

            var addressPostcodeMessage = string.Empty;
            var addressPostcodeValid = true;

            if (string.IsNullOrEmpty(valueAddressPostcode))
            {
                addressPostcodeMessage = "Please enter a Postcode";
                addressPostcodeValid = false;
            }
            else if (!_stockportPostCode.IsMatch(valueAddressPostcode) && element.Properties.StockportPostcode == true)
            {
                addressPostcodeMessage = "Please enter a valid Stockport Postcode";
                addressPostcodeValid = false;
            }
            else if(!_postCode.IsMatch(valueAddressPostcode))
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
