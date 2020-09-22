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
            if (element.Type != EElementType.FileUpload && element.Type != EElementType.MultipleFileUpload || viewModel.ContainsKey(ButtonConstants.NoDataSubmit))
            {              
                    return new ValidationResult
                    {
                        IsValid = true
                    };                
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
            
            var allowedFileTypes = element.Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;

            var fileTypes = documentModel.Select(_ => new MimeTypeFile{ FileType= Convert.FromBase64String(_.Content).GetFileType(), File= _ });
            var invalidFiles = new List<MimeTypeFile>();
            if (fileTypes != null)
            {
                var availableFileTypes = fileTypes.Where(_ => _ != null);
                invalidFiles = availableFileTypes.Where(_ => !allowedFileTypes.Contains($".{_.FileType.Extension}")).ToList();
                if (!invalidFiles.Any())
                {
                    return new ValidationResult
                    {
                        IsValid = true
                    };
                }
            }

            return element.Type == EElementType.FileUpload
               ? SingleFileUpload(allowedFileTypes, documentModel)
               : MultiFileUpload(allowedFileTypes, invalidFiles);            
        }

        private ValidationResult MultiFileUpload(List<string> allowedFileTypes, List<MimeTypeFile> invalidFiles)
        {
            var fileTypesErrorMessage = GenerateErrorMessage(allowedFileTypes);

            var validationMessage = invalidFiles.Count == 1
                ? fileTypesErrorMessage
                : invalidFiles.Select(_ => $"{_.File.FileName} must be a {fileTypesErrorMessage}").Aggregate((curr, acc) => $" {acc} <br/> {curr} ");

            return new ValidationResult
            {
                IsValid = false,
                Message = $"The selected file must be a {fileTypesErrorMessage.Replace(".", string.Empty)}."
            };
        }

        private ValidationResult SingleFileUpload(List<string> allowedFileTypes, List<DocumentModel> documentModel)
        {
            var fileTypesErrorMessage = GenerateErrorMessage(allowedFileTypes);

            return new ValidationResult
            {
                IsValid = false,
                Message = $"The selected file must be a {fileTypesErrorMessage.Replace(".", string.Empty)}."
            };
        }

        private static string GenerateErrorMessage(List<string> allowedFileTypes)
        {
            return allowedFileTypes.Count > 1
                  ? string.Join(", ", allowedFileTypes.Take(allowedFileTypes.Count - 1)) + $" or {allowedFileTypes.Last()}"
                  : allowedFileTypes.First();
        }
    }
}