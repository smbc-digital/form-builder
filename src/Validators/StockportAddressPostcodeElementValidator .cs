using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.Models.Elements;
using form_builder.Enum;

namespace form_builder.Validators
{
    public class StockportAddressPostcodeElementValidator : IElementValidator
    {
       
        
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {

            if (element.Type != EElementType.Address)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (element.Properties.StockportPostcode != true)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }
          

            if ((!element.Properties.StockportPostcode.HasValue || !element.Properties.StockportPostcode.Value) || !viewModel.ContainsKey(element.Properties.QuestionId + "-postcode"))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (string.IsNullOrEmpty(viewModel[element.Properties.QuestionId + "-postcode"]) && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var value = viewModel[element.Properties.QuestionId + "-postcode"];

            var isValid = true;
            var regex = new Regex(@"^(sK|Sk|SK|sk|M|m)[0-9][0-9A-Za-z]?\s?[0-9][A-Za-z]{2}");

            Match match = regex.Match(value);

            if (!match.Success)
            {
                isValid = false;
            }

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : $"{ element.Properties.Label} must be a valid Stockport postcode"
            };
        }
    }
}