using System;
using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.Validators
{
    public class RequiredElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {

            if ((element.Properties.Optional.HasValue && element.Properties.Optional.Value))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var key = element.Properties.QuestionId;

            var validationMessage = $"{element.Properties.Label} is required";

            if (element.Type == Enum.EElementType.Address)
            {
                if (viewModel["AddressStatus"] == "Select")
                {
                    key = $"{element.Properties.QuestionId}-address";
                    validationMessage = $"{ element.Properties.AddressLabel} is required";
                }
                else
                {
                    key = $"{element.Properties.QuestionId}-postcode";
                    validationMessage = $"{ element.Properties.PostcodeLabel} is required";
                }
            }

            var value = viewModel.ContainsKey(key)
                ? viewModel[key]
                : null;

            var isValid = !string.IsNullOrEmpty(value);

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : validationMessage
            };
        }
    }
}