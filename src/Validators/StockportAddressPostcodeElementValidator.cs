using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class StockportAddressPostcodeElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (element.Type != EElementType.Address || (element.Type == EElementType.Address && !viewModel.IsInitial()))
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

            if ((!element.Properties.StockportPostcode.HasValue || !element.Properties.StockportPostcode.Value) || !viewModel.ContainsKey($"{element.Properties.QuestionId}{AddressConstants.SEARCH_SUFFIX}"))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (string.IsNullOrEmpty(viewModel[$"{element.Properties.QuestionId}{AddressConstants.SEARCH_SUFFIX}"]) && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var value = viewModel[$"{element.Properties.QuestionId}{AddressConstants.SEARCH_SUFFIX}"];
            var isValid = true;
            
            if (!AddressConstants.STOCKPORT_POSTCODE_REGEX.Match(value).Success)
            {
                isValid = false;
            }

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : ValidationConstants.POSTCODE_INCORRECT_FORMAT
            };
        }
    }
}