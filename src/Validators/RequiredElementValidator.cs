using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class RequiredElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (element.Type.Equals(EElementType.DateInput) || element.Type.Equals(EElementType.Map) ||
                element.Type.Equals(EElementType.TimeInput) || element.Type.Equals(EElementType.DatePicker) ||
                element.Type.Equals(EElementType.MultipleFileUpload) || element.Properties.Optional ||
                element.Type.Equals(EElementType.Booking) || element.Type.Equals(EElementType.AddAnother) ||
                (element.Type.Equals(EElementType.Address) && viewModel.IsManual()))
            {
                return new ValidationResult { IsValid = true };
            }

            var key = element.Properties.QuestionId;
            var validationMessage = string.Empty;

            if (!element.Type.Equals(EElementType.Address) && !element.Type.Equals(EElementType.Street) && !element.Type.Equals(EElementType.Organisation))
                validationMessage = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Enter the " + element.Properties.Label.ToLower();

            if (element.Type.Equals(EElementType.FileUpload))
                key = $"{element.Properties.QuestionId}{FileUploadConstants.SUFFIX}";

            if (element.Type.Equals(EElementType.Address))
            {
                if (viewModel.IsAutomatic())
                {
                    key = $"{element.Properties.QuestionId}-address";
                    validationMessage = !string.IsNullOrEmpty(element.Properties.SelectCustomValidationMessage) ? element.Properties.SelectCustomValidationMessage : "Select an address from the list";
                }
                else
                {
                    key = $"{element.Properties.QuestionId}-postcode";
                    validationMessage = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : ValidationConstants.POSTCODE_EMPTY;

                }
            }

            if (element.Type.Equals(EElementType.Street))
            {
                if (viewModel.IsAutomatic())
                {
                    key = $"{element.Properties.QuestionId}-street";
                    validationMessage = !string.IsNullOrEmpty(element.Properties.SelectCustomValidationMessage) ? element.Properties.SelectCustomValidationMessage : "Select the street from the list";
                }
                else
                {
                    key = $"{element.Properties.QuestionId}";
                    validationMessage = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Enter the street name";
                }
            }

            if (element.Type.Equals(EElementType.Organisation))
            {
                if (viewModel.IsAutomatic())
                {
                    key = $"{element.Properties.QuestionId}-organisation";
                    validationMessage = !string.IsNullOrEmpty(element.Properties.SelectCustomValidationMessage) ? element.Properties.SelectCustomValidationMessage : "Select the organisation from the list";
                }
                else
                {
                    key = $"{element.Properties.QuestionId}";
                    validationMessage = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Enter the organisation name";
                }
            }

            bool isValid;
            if (element.Type.Equals(EElementType.FileUpload))
            {
                List<DocumentModel> value = viewModel.ContainsKey(key)
                    ? viewModel[key]
                    : null;

                isValid = !(value is null);
                validationMessage = ValidationConstants.FILEUPLOAD_EMPTY;
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