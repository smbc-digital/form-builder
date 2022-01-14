using System.Threading.Tasks;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class DateValidationsCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (element.Properties is null)
                return result;

            if (!string.IsNullOrEmpty(element.Properties.IsDateAfter) &&
                element.Properties.QuestionId.Equals(element.Properties.IsDateAfter))
            {
                result.AddFailureMessage(
                    "Date Validations Check, " +
                    $"IsDateAfter validation, for question '{element.Properties.QuestionId}' - " +
                    $"the form does not contain a comparison element with QuestionId '{element.Properties.IsDateAfter}'");
            }

            if (!string.IsNullOrEmpty(element.Properties.IsDateBefore) &&
                element.Properties.QuestionId.Equals(element.Properties.IsDateBefore))
            {
                result.AddFailureMessage(
                    "Date Validations Check, " +
                    $"IsDateBefore validation, for question '{element.Properties.QuestionId}' - " +
                    $"the form does not contain a comparison element with question id '{element.Properties.IsDateBefore}'");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}