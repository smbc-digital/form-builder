using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using MimeDetective;
using Newtonsoft.Json;
using System.Text;
using form_builder.Constants;
using System;

namespace form_builder.Validators
{
    public class RestrictMimeTypeValidator : IElementValidator
    {

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {

            if(element.Type != EElementType.FileUpload)
            {
                return new ValidationResult { IsValid = true };
            }

            if (!viewModel.ContainsKey(element.Properties.QuestionId))
            {
                return new ValidationResult { IsValid = true };
            }

            DocumentModel documentModel = viewModel[element.Properties.QuestionId];

            var converedtBase64File = Convert.FromBase64String(documentModel.Content);
            var fileType = converedtBase64File.GetFileType();
            var allowedFileTypes = element.Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;

            if (allowedFileTypes.Contains($".{fileType.Extension}"))
            {
                return new ValidationResult { IsValid = true };
            }

            return new ValidationResult { IsValid = false, Message = "This file type is not allowed" };
        }
    }
}
