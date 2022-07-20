using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class AddressPostcodeValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.Address) || (element.Type.Equals(EElementType.Address) && !viewModel.IsInitial()))
                return new ValidationResult(true);

            var addressElement = (Address)element;

            if (!viewModel.ContainsKey(addressElement.AddressSearchQuestionId))
                return new ValidationResult(true);

            if (!element.Properties.ValidatePostcode)
                return new ValidationResult(true);

            if (string.IsNullOrEmpty(viewModel[addressElement.AddressSearchQuestionId]) && element.Properties.Optional)
                return new ValidationResult(true);

            var isValid = AddressConstants.POSTCODE_REGEX
                            .Match(viewModel[addressElement.AddressSearchQuestionId])
                            .Success;

            return new ValidationResult(isValid, isValid 
                                                    ? string.Empty 
                                                    : ValidationConstants.POSTCODE_INCORRECT_FORMAT);
        }
    }
}