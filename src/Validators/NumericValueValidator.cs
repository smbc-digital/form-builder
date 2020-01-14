using System.Collections.Generic;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class NumericValueValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {
            if (!element.Properties.Numeric || !viewModel.ContainsKey(element.Properties.QuestionId))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var value = viewModel[element.Properties.QuestionId];

            if(string.IsNullOrEmpty(value) && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var isValid = int.TryParse(value, out int output);

            if (!isValid)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"{element.Properties.Label} must be a number"
                };
            }

            return new ValidationResult
            {
                IsValid = true
            };
        }
    }
}