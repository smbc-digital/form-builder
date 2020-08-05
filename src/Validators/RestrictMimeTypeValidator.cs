using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using MimeDetective;

namespace form_builder.Validators
{
    public class RestrictMimeTypeValidator : IElementValidator
    {

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if (element.Type != EElementType.FileUpload)
            {
                return new ValidationResult { IsValid = true };
            }

            var key = $"{element.Properties.QuestionId}-fileupload";

            if (!viewModel.ContainsKey(key))
            {
                return new ValidationResult { IsValid = true };
            }

            DocumentModel documentModel = viewModel[key];

            if (documentModel == null)
            {
                return new ValidationResult { IsValid = true };
            }

            var converedtBase64File = Convert.FromBase64String(documentModel.Content);
            var fileType = converedtBase64File.GetFileType();
            var allowedFileTypes = element.Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;

            if (fileType != null)
            {
                if (allowedFileTypes.Contains($".{fileType.Extension}"))
                {
                    return new ValidationResult { IsValid = true };
                }
            }

            var fileTypesErrorMessage = allowedFileTypes.Count > 1
                ? string.Join(", ", allowedFileTypes.Take(allowedFileTypes.Count - 1)).ToString() + $" or {allowedFileTypes.Last().ToString()}"
                : allowedFileTypes.First().ToString();

            return new ValidationResult { IsValid = false, Message = $"The selected file must be a {fileTypesErrorMessage.Replace(".", string.Empty)}." };
        }
    }
}
