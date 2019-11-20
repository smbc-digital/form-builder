using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.Models;

namespace form_builder.Validators
{
    public class EmailElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {
            if((!element.Properties.Email.HasValue || !element.Properties.Email.Value) || !viewModel.ContainsKey(element.Properties.QuestionId))
            {
                return new ValidationResult{
                    IsValid = true
                };
            }
        
            var value = viewModel[element.Properties.QuestionId];

            var isValid = true;
            var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

            Match match = regex.Match(value);

            if (!match.Success)
            {
                isValid = false;
            }

            return new ValidationResult{
                    IsValid = isValid,
                    Message = isValid ? string.Empty : $"{ element.Properties.Label} must be a valid email address"
                }; 
        }
    }
}