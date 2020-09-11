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
            if (element.Type != EElementType.FileUpload  || element.Type != EElementType.MultipleFileUpload))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var key = $"{element.Properties.QuestionId}-fileupload";

            if (!viewModel.ContainsKey(key))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            List<DocumentModel> documentModel = viewModel[key];

            if (documentModel == null)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var convertedBase64File = documentModel.Select(_ => Convert.FromBase64String(_.Content));
            var fileType = convertedBase64File.Select(_ => _.GetFileType());
            var allowedFileTypes = element.Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;

            if (fileType != null)
            {
                if (fileType.All(_ => allowedFileTypes.Contains($".{_.Extension}")))
                {
                    return new ValidationResult
                    {
                        IsValid = true
                    };
                }
            }

            var fileTypesErrorMessage = allowedFileTypes.Count > 1
                ? string.Join(", ", allowedFileTypes.Take(allowedFileTypes.Count - 1)) + $" or {allowedFileTypes.Last()}"
                : allowedFileTypes.First();

            return new ValidationResult
            {
                IsValid = false,
                Message = $"The selected file must be a {fileTypesErrorMessage.Replace(".", string.Empty)}."
            };
        }
    }
}