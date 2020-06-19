using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class EmailElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if(element.Properties.Email != true)
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

            if ((!element.Properties.Email.HasValue || !element.Properties.Email.Value) || !viewModel.ContainsKey(element.Properties.QuestionId))
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
                    Message = isValid ? string.Empty : "Enter an email address in the correct format, like name@example.com"
                }; 
        }
    }
}