using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class RequiredElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {

            if (element.Type == EElementType.DateInput || element.Type == EElementType.TimeInput ||
                element.Type == EElementType.AddressManual || element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var key = element.Properties.QuestionId;

            var validationMessage = string.Empty;

            if (element.Type != EElementType.Address && element.Type != EElementType.Street && element.Type != EElementType.Organisation)
            {
                validationMessage = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Check the " + element.Properties.Label.ToLower() + " and try again";
            }

            if (element.Type == EElementType.Address)
            {
                if (viewModel["AddressStatus"] == "Select")
                {
                    key = $"{element.Properties.QuestionId}-address";
                    validationMessage = "Check the " + element.Properties.AddressLabel.ToLower() + " and try again";
                }
                else
                {
                    key = $"{element.Properties.QuestionId}-postcode";
                    validationMessage = "Check the " + element.Properties.PostcodeLabel.ToLower() + " and try again";
                }
            }

            if (element.Type == EElementType.Street)
            {
                if (viewModel["StreetStatus"] == "Select")
                {
                    key = $"{element.Properties.QuestionId}-streetaddress";
                    validationMessage = "Check the " + element.Properties.SelectLabel.ToLower() + " and try again";
                }
                else
                {
                    key = $"{element.Properties.QuestionId}-street";
                    validationMessage = "Check the " + element.Properties.StreetLabel.ToLower() + " and try again";
                }
            }

            if (element.Type == EElementType.Organisation)
            {
                if (viewModel["OrganisationStatus"] == "Select")
                {
                    key = $"{element.Properties.QuestionId}-organisation";
                    validationMessage = "Check the " + element.Properties.SelectLabel.ToLower() + " and try again";
                }
                else
                {
                    key = $"{element.Properties.QuestionId}-organisation-searchterm";
                    validationMessage = "Check the " + element.Properties.Label.ToLower() + " and try again";
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