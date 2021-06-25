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
            if (!(bool)element.Properties.Postcode)
                return new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(viewModel[element.Properties.QuestionId]) && element.Properties.Optional)
                return new ValidationResult { IsValid = true };

            if ((!element.Properties.Postcode.HasValue || !element.Properties.Postcode.Value) || !viewModel.ContainsKey(element.Properties.QuestionId))
                return new ValidationResult { IsValid = true };

            var value = viewModel[element.Properties.QuestionId];
            var isValid = true;
            if (!AddressConstants.POSTCODE_REGEX.Match(value).Success)
                isValid = false;

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : ValidationConstants.POSTCODE_INCORRECT_FORMAT
            };
        }
    }
}