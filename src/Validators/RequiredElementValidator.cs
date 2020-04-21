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
                var addressElement = (Address)element;

                if (viewModel.ContainsKey("AddressStatus") &&viewModel["AddressStatus"] == "Select")
                {
                    key = addressElement.AddressSelectQuestionId;
                    validationMessage = "Check the " + element.Properties.SelectLabel.ToLower() + " and try again";
                }
                else
                {
                    key = addressElement.AddressSearchQuestionId;
                    validationMessage = "Check the " + element.Properties.PostcodeLabel.ToLower() + " and try again";
                }
            }

            if (element.Type == EElementType.Street)
            {
                var streetElement = (Street)element;
                if (viewModel["StreetStatus"] == "Select")
                {
                    key = streetElement.StreetSelectQuestionId;
                    validationMessage = "Check the " + element.Properties.SelectLabel.ToLower() + " and try again";
                }
                else
                {
                    key = streetElement.StreetSearchQuestionId;
                    validationMessage = "Check the " + element.Properties.Label.ToLower() + " and try again";
                }
            }

            if (element.Type == EElementType.Organisation)
            {
                var organisationElement = (Organisation)element;
                if (viewModel["OrganisationStatus"] == "Select")
                {
                    key = organisationElement.OrganisationSelectQuestionId;
                    validationMessage = "Check the " + element.Properties.SelectLabel.ToLower() + " and try again";
                }
                else
                {
                    key = organisationElement.OrganisationSearchQuestionId;
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