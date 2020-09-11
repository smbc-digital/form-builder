using System.Collections.Generic;
using System.Linq;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class RestrictFileSizeValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if (element.Type != EElementType.FileUpload || element.Type != EElementType.MultipleFileUpload)
                return new ValidationResult { IsValid = true };

            var key = $"{element.Properties.QuestionId}-fileupload";

            if (!viewModel.ContainsKey(key))
                return new ValidationResult { IsValid = true };

            List<DocumentModel> documentModel = viewModel[key];

            if (documentModel == null)
                return new ValidationResult { IsValid = true };

            var maxFileSize = element.Properties.MaxFileSize > 0 ? element.Properties.MaxFileSize * 1048576 : SystemConstants.DefaultMaxFileSize;

            if (documentModel.All(_ => _.FileSize <= maxFileSize))
                return new ValidationResult { IsValid = true };

            return new ValidationResult
            {
                IsValid = false, Message = $"The selected file must smaller than {maxFileSize / 1048576} MB"
            };
        }
    }
}