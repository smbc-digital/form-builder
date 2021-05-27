using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class MaxLengthValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!viewModel.ContainsKey(element.Properties.QuestionId))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (element.Type == EElementType.FileUpload || element.Type == EElementType.Map || element.Type == EElementType.Checkbox)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var value = viewModel.ContainsKey(element.Properties.QuestionId) ? viewModel[element.Properties.QuestionId] : "";

            if (!string.IsNullOrEmpty(value) && value.Length > element.Properties.MaxLength)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"{element.Properties.Label} has a maximum length of {element.Properties.MaxLength}"
                };
            }

            return new ValidationResult
            {
                IsValid = true
            };
        }
    }
}