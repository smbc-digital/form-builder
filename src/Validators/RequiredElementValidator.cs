using System;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators
{
    public class RequiredElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {

            if (element.Type == EElementType.DateInput || element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var value = viewModel.ContainsKey(element.Properties.QuestionId)
                ? viewModel[element.Properties.QuestionId]
                : null;

            var isValid = !string.IsNullOrEmpty(value);

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : $"{element.Properties.Label} is required"
            };
        }
    }
}