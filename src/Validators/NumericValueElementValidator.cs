using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.Validators
{
    public class NumericValueElementValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {
            if((!element.Properties.Numeric) || !viewModel.ContainsKey(element.Properties.QuestionId))
            {
                return new ValidationResult{
                    IsValid = true
                };
            }
        
            var value = viewModel[element.Properties.QuestionId];
            var isValid = int.TryParse(value, out int output);

            return new ValidationResult{
                    IsValid = isValid,
                    Message = isValid ? string.Empty : $"{element.Properties.Label} must be a number"
                }; 
        }
    }
}