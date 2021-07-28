using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models.Elements;
using System.Linq;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class CheckboxElementCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (!element.Type.Equals(EElementType.Checkbox))
                return result;

            if(element.Properties.Options.Any(_ => _.Exclusive))
            {
                var optionsWhichAreExclusive = element.Properties.Options.Where(_ => _.Exclusive).ToList();

                if(optionsWhichAreExclusive.Count > 1)
                    result.AddFailureMessage($"Checkbox Element Check: {element.Properties.QuestionId} contains multiple options found with exclusive set to 'true', only a single one can be exclusive");

                if(string.IsNullOrEmpty(element.Properties.ExclusiveCheckboxValidationMessage))
                    result.AddFailureMessage($"Checkbox Element Check: You must provide a validation message when you have options which are exclsuive, Set 'ExclusiveCheckboxValidationMessage' property within element with questionId {element.Properties.QuestionId}");
            }
            
            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}