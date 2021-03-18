using form_builder.Models.Elements;
using System;
using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class AbsoluteDateValidationsCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            if (element.Properties is null)
                return integrityCheckResult;

            if (!string.IsNullOrEmpty(element.Properties.IsDateAfterAbsolute) &&
                !DateTime.TryParse(element.Properties.IsDateAfterAbsolute, out DateTime _))
            {
                integrityCheckResult.AddFailureMessage($"Absolute Date Validations Check, IsDateAfterAbsolute validation, '{element.Properties.QuestionId}' does not provide a valid comparison date.");
            }

            if (!string.IsNullOrEmpty(element.Properties.IsDateBeforeAbsolute) &&
            !DateTime.TryParse(element.Properties.IsDateBeforeAbsolute, out DateTime _))
            {
                integrityCheckResult.AddFailureMessage($"Absolute Date Validations Check, IsDateBeforeAbsolute validation, '{element.Properties.QuestionId}' does not provide a valid comparison date");
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}