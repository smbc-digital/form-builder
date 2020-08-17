using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class RegexElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if(string.IsNullOrEmpty(element.Properties.Regex))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (string.IsNullOrEmpty(viewModel[element.Properties.QuestionId]) && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (string.IsNullOrEmpty(element.Properties.Regex) || !viewModel.ContainsKey(element.Properties.QuestionId))
            {
                return new ValidationResult{
                    IsValid = true
                };
            }
        
            var value = viewModel[element.Properties.QuestionId];

            var isValid = true;
            var regex = new Regex(element.Properties.Regex);

            Match match = regex.Match(value);

            if (!match.Success)
            {
                isValid = false;
            }

            return new ValidationResult{
                    IsValid = isValid,
                    Message = isValid ? string.Empty : !string.IsNullOrEmpty(element.Properties.RegexValidationMessage) ? element.Properties.RegexValidationMessage : $"Check the { element.Properties.Label} and try again"
                }; 
        }
    }
}