using System;
using System.Threading.Tasks;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class AbsoluteDateValidationsCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (element.Properties is null)
                return result;

            if (!string.IsNullOrEmpty(element.Properties.IsDateAfterAbsolute) &&
                !DateTime.TryParse(element.Properties.IsDateAfterAbsolute, out DateTime _))
                    result.AddFailureMessage($"Absolute Date Validations Check, IsDateAfterAbsolute validation, '{element.Properties.QuestionId}' does not provide a valid comparison date.");

            if (!string.IsNullOrEmpty(element.Properties.IsDateBeforeAbsolute) &&
                !DateTime.TryParse(element.Properties.IsDateBeforeAbsolute, out DateTime _))
                    result.AddFailureMessage($"Absolute Date Validations Check, IsDateBeforeAbsolute validation, '{element.Properties.QuestionId}' does not provide a valid comparison date");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}