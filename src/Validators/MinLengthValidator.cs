using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators;

public class MinLengthValidator : IElementValidator
{
    public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
    {
        if (!viewModel.ContainsKey(element.Properties.QuestionId))
            return new ValidationResult { IsValid = true };

        // Currently this is only applied to Textbox type elements
        if (!element.Type.Equals(EElementType.Textbox) 
            || element.Properties.MinLength is null 
            || element.Properties.Decimal 
            || element.Properties.Numeric 
            || element.Properties.Optional)
            return new ValidationResult { IsValid = true };

        var value = viewModel.ContainsKey(element.Properties.QuestionId) ? viewModel[element.Properties.QuestionId] : "";

        if (!string.IsNullOrEmpty(value) && value.Length < element.Properties.MinLength)
        {
            var validationMessage = $"Enter {element.Properties.MinLength} or more characters";

            return new ValidationResult
            {
                IsValid = false,
                Message = validationMessage
            };
        }

        return new ValidationResult { IsValid = true };
    }
}
