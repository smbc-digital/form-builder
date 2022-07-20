using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class PostcodeElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {                        
            if (!element.Properties.Postcode)
                return new ValidationResult(true);

            if (string.IsNullOrEmpty(viewModel[element.Properties.QuestionId]) && element.Properties.Optional)
                return new ValidationResult(true);

            if (!viewModel.ContainsKey(element.Properties.QuestionId))
                return new ValidationResult(true);

            var isValid = AddressConstants.POSTCODE_REGEX
                                            .Match(viewModel[element.Properties.QuestionId])
                                            .Success;

            return new ValidationResult(isValid, isValid 
                                                    ? string.Empty 
                                                    : ValidationConstants.POSTCODE_INCORRECT_FORMAT);
        }
    }
}