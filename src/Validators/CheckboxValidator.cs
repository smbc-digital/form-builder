using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class CheckboxValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.Checkbox) && !element.Type.Equals(EElementType.Declaration))
                return new ValidationResult { IsValid = true };

            var containsValue = viewModel.ContainsKey(element.Properties.QuestionId);
            if (!containsValue && element.Properties.Optional)
                return new ValidationResult { IsValid = true };

            dynamic value = viewModel[element.Properties.QuestionId] as List<string> ?? viewModel[element.Properties.QuestionId];
            if (value is null)
                return new ValidationResult { IsValid = false };

            return new ValidationResult { IsValid = true };
        }
    }
}