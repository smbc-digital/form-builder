using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class RequiredElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {

            if (element.Type == EElementType.DateInput || element.Type == EElementType.AddressManual || element.Properties.Optional)
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

            if (element.Type == EElementType.Street)
            {
                if (viewModel["StreetStatus"] == "Select")
                {
                    key = $"{element.Properties.QuestionId}-street";
                    validationMessage = $"{ element.Properties.SelectLabel} is required";
                }
                else
                {
                    key = $"{element.Properties.QuestionId}-street";
                    validationMessage = $"{ element.Properties.StreetLabel} is required";
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