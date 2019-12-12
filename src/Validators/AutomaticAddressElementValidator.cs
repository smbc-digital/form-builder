using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class AutomaticAddressElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {
            if (!viewModel.ContainsKey($"{element.Properties.QuestionId}-address"))
            {
                return new ValidationResult{
                    IsValid = true
                };
            }

            var value = viewModel[$"{element.Properties.QuestionId}-address"];

            if (element.Properties.Optional && string.IsNullOrEmpty(value))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }


            var isValid = Regex.IsMatch(value, "^[0-9]{12}$"); 

            return new ValidationResult{
                    IsValid = isValid,
                    Message = isValid ? string.Empty : $"please select an address"
                }; 
        }
    }
}