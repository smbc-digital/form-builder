using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class ManualAddressValidator : IElementValidator
    {
        private readonly Regex _postCode  = new Regex(@"^([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})$");
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {

            if (element.Type != EElementType.AddressManual)
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

            var addressLine1Message = addressLine1Valid ? string.Empty : "Please Enter Address Line 1";



            var valueAddressTown = viewModel.ContainsKey($"{element.Properties.QuestionId}-AddressManualAddressTown")
                ? viewModel[$"{element.Properties.QuestionId}-AddressManualAddressTown"]
                : null;

            var addressTownValid = !string.IsNullOrEmpty(valueAddressTown);
            var addressTownMessage = addressTownValid ? string.Empty : "Please Enter Town";

            var valueAddressPostcode = viewModel.ContainsKey($"{element.Properties.QuestionId}-AddressManualAddressPostcode")
                ? viewModel[$"{element.Properties.QuestionId}-AddressManualAddressPostcode"]
                : null;

            var addressPostcodeValid = !string.IsNullOrEmpty(valueAddressPostcode) && _postCode.IsMatch(valueAddressPostcode);
           

            var addressPostcodeMessage = addressPostcodeValid ? string.Empty : "Please Enter Postcode";

            var isValid = addressLine1Valid && addressTownValid && addressPostcodeValid;

          

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : $"{addressLine1Message}, {addressTownMessage}, {addressPostcodeMessage}"
            };
        }
    }
}
