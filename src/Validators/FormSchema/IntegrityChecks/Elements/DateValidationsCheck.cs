using form_builder.Models.Elements;
using form_builder.Validators.IntegrityChecks.Elements;
using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class DateValidationsCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            if (element.Properties is null)
                return integrityCheckResult;

            if (!string.IsNullOrEmpty(element.Properties.IsDateAfter) &&
                element.Properties.QuestionId.Equals(element.Properties.IsDateAfter))
                integrityCheckResult.AddFailureMessage($"Date Validations Check, IsDateAfter validation, for question '{element.Properties.QuestionId}' - the form does not contain a comparison element with QuestionId '{element.Properties.IsDateAfter}'");

            if (!string.IsNullOrEmpty(element.Properties.IsDateBefore) &&
                element.Properties.QuestionId.Equals(element.Properties.IsDateAfter))
                integrityCheckResult.AddFailureMessage($"Date Validations Check, IsDateBefore validation, for question '{element.Properties.QuestionId}' - the form does not contain a comparison element with question id '{element.Properties.IsDateBefore}'");

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}