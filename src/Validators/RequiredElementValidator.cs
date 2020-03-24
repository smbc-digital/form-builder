using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models.Elements;
using form_builder.Models;

namespace form_builder.Validators
{
    public class RequiredElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {

            if (element.Type == EElementType.DateInput || element.Type == EElementType.TimeInput ||
                element.Type == EElementType.AddressManual || element.Type == EElementType.DatePicker ||
                element.Properties.Optional)
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

            if (element.Type == EElementType.FileUpload)
            {
                key = $"{element.Properties.QuestionId}-fileupload";
            }

            if (element.Type == EElementType.Address)
            {
                if (viewModel.ContainsKey("AddressStatus") &&viewModel["AddressStatus"] == "Select")
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

            var isValid = false;

            if (element.Type == EElementType.FileUpload)
            {
                DocumentModel value = viewModel.ContainsKey(key)
                    ? viewModel[key]
                    : null;

                isValid = !(value is null);
            }
            else
            {
                var value = viewModel.ContainsKey(key)
                ? viewModel[key]
                : null;
                isValid = !string.IsNullOrEmpty(value);
            }
            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : validationMessage
            };
        }
    }
}