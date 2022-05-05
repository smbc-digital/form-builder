using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;
using MimeDetective;

namespace form_builder.Validators
{
    public class RestrictMimeTypeValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.FileUpload) && !element.Type.Equals(EElementType.MultipleFileUpload))
                return new ValidationResult { IsValid = true };

            var key = $"{element.Properties.QuestionId}{FileUploadConstants.SUFFIX}";

            if (!viewModel.ContainsKey(key))
                return new ValidationResult { IsValid = true };

            List<DocumentModel> documentModel = viewModel[key];

            if (documentModel is null)
                return new ValidationResult { IsValid = true };

            var allowedFileTypes = element.Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;

            var fileTypes = documentModel.Select(_ => new MimeTypeFile { FileType = Convert.FromBase64String(_.Content).GetFileType(), File = _ });
            var invalidFiles = new List<MimeTypeFile>();
            if (fileTypes is not null)
            {
                var availableFileTypes = fileTypes.Where(_ => _ is not null && _.FileType is not null);
                var unknownFileTypes = fileTypes.Where(_ => _ is not null && _.FileType is null);

                if (unknownFileTypes.Any())
                    invalidFiles.AddRange(unknownFileTypes.ToList());

                invalidFiles.AddRange(availableFileTypes.Where(_ => !allowedFileTypes.Contains($".{_.FileType.Extension}") || !allowedFileTypes.Contains(Path.GetExtension(_.File.FileName.ToLower()))).ToList());
                if (!invalidFiles.Any())
                    return new ValidationResult { IsValid = true };
            }

            return element.Type.Equals(EElementType.FileUpload)
               ? SingleFileUpload(allowedFileTypes, documentModel)
               : MultiFileUpload(allowedFileTypes, invalidFiles, documentModel);
        }

        private ValidationResult MultiFileUpload(List<string> allowedFileTypes, List<MimeTypeFile> invalidFiles, List<DocumentModel> documentModel)
        {
            var fileTypesErrorMessage = allowedFileTypes.ToReadableFileType("or");

            var validationMessage = documentModel.Count.Equals(1)
                ? $"The selected file must be a {fileTypesErrorMessage}."
                : invalidFiles.Select(_ => $"{_.File.FileName} must be a {fileTypesErrorMessage}.").Aggregate((curr, acc) => $"{acc} <br/> {curr}");

            return new ValidationResult
            {
                IsValid = false,
                Message = validationMessage
            };
        }

        private ValidationResult SingleFileUpload(List<string> allowedFileTypes, List<DocumentModel> documentModel)
        {
            var fileTypesErrorMessage = allowedFileTypes.ToReadableFileType("or");

            return new ValidationResult
            {
                IsValid = false,
                Message = $"The selected file must be a {fileTypesErrorMessage}."
            };
        }
    }
}