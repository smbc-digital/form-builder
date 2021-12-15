using System.Collections.Generic;
using System.Linq;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class ExclusiveCheckboxValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.Checkbox))
                return new ValidationResult { IsValid = true };

            if (!viewModel.ContainsKey(element.Properties.QuestionId))
                return new ValidationResult { IsValid = true };

            var answer = (string[])viewModel[element.Properties.QuestionId].Split(",");

            if (answer.Length < 2)
                return new ValidationResult { IsValid = true };

            if (!element.Properties.Options.ToList().Any(_ => _.Exclusive))
                return new ValidationResult { IsValid = true };

            bool isValid = true;
            answer.ToList().ForEach(value =>
            {
                if (element.Properties.Options.FirstOrDefault(_ => _.Exclusive && _.Value.Equals(value)) is not null)
                    isValid = false;
            });

            return new ValidationResult { IsValid = isValid, Message = isValid ? string.Empty : element.Properties.ExclusiveCheckboxValidationMessage };
        }
    }
}
