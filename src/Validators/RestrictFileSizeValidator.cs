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

            return element.Type.Equals(EElementType.FileUpload)
                ? SingleFileUpload(element, documentModel)
                : MultiFileUpload(element, documentModel);
        }

        private ValidationResult MultiFileUpload(Element element, List<DocumentModel> documentModel)
        {
            var maxFileSize = element.Properties.MaxFileSize > 0 ? element.Properties.MaxFileSize * SystemConstants.OneMBInBinaryBytes : SystemConstants.DefaultMaxFileSize;

            var invalidFileSizes = documentModel.Where(_ => _.FileSize >= maxFileSize).ToList();

            if (!invalidFileSizes.Any())
                return new ValidationResult { IsValid = true };

            var validationMessage = documentModel.Count.Equals(1)
                ? $"The selected file must be smaller than {maxFileSize / SystemConstants.OneMBInBinaryBytes}MB"
                : invalidFileSizes.Select(_ => $"{_.FileName} must be smaller than {maxFileSize / SystemConstants.OneMBInBinaryBytes}MB").Aggregate((curr, acc) => $"{acc} <br/> {curr}");

            return new ValidationResult
            {
                IsValid = false,
                Message = validationMessage
            };
        }

        private ValidationResult SingleFileUpload(Element element, List<DocumentModel> documentModel)
        {
            var maxFileSize = element.Properties.MaxFileSize > 0 ? element.Properties.MaxFileSize * SystemConstants.OneMBInBinaryBytes : SystemConstants.DefaultMaxFileSize;

            if (documentModel.All(_ => _.FileSize <= maxFileSize))
                return new ValidationResult { IsValid = true };

            return new ValidationResult
            {
                IsValid = false,
                Message = $"The selected file must be smaller than {maxFileSize / SystemConstants.OneMBInBinaryBytes}MB"
            };
        }
    }
}