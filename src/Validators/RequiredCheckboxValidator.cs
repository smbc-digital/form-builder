using System.Collections.Generic;
using System.Linq;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class ExactNumberOptionsCheckboxValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.Checkbox))
                return new ValidationResult { IsValid = true };

            if (!viewModel.ContainsKey(element.Properties.QuestionId))
                return new ValidationResult { IsValid = true };


            if(element.Properties.ExactNumberOptions < 0)
                return new ValidationResult { IsValid = true };

            var answer = (string[])viewModel[element.Properties.QuestionId].Split(",");

            bool isValid = true;
            if (answer.Length != element.Properties.ExactNumberOptions)
                isValid = false;

            return new ValidationResult { IsValid = isValid, Message = isValid ? string.Empty : element.Properties.CustomValidationMessage};
        }
    }
}
