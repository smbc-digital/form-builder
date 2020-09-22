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
                if (element.Type != EElementType.MultipleFileUpload || viewModel.ContainsKey(ButtonConstants.NoDataSubmit))
                {
                    return new ValidationResult
                    {
                        IsValid = true
                    };
                }                
            }

            var key = $"{element.Properties.QuestionId}{FileUploadConstants.SUFFIX}";

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

            var documentsFiles = documentModel.Select(_ =>  Convert.FromBase64String(_.Content));
            
            var allowedFileTypes = element.Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;

            var fileTypes = documentsFiles.Select(_ => _.GetFileType());
            if (fileTypes != null)
            {
                var availableFileTypes = fileTypes.Where(_ => _ != null);
                if (availableFileTypes.Any() && availableFileTypes.All(_ => allowedFileTypes.Contains($".{_.Extension}")))
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