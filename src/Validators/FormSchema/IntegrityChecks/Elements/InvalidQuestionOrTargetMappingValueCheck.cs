using form_builder.Models.Elements;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class InvalidQuestionOrTargetMappingValueCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            if (element.Properties is null)
                return integrityCheckResult;

            var regex = new Regex(@"^[a-zA-Z.]+$", RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(element.Properties.QuestionId) &&
                !regex.IsMatch(element.Properties.QuestionId))
            {
                integrityCheckResult.AddFailureMessage($"The provided json contains invalid QuestionIDs, '{element.Properties.QuestionId}' contains invalid characters");
            }

            if (!string.IsNullOrEmpty(element.Properties.TargetMapping))
            {
                if ((!regex.IsMatch(element.Properties.TargetMapping))
                    || element.Properties.TargetMapping.EndsWith(".")
                    || element.Properties.TargetMapping.StartsWith("."))
                {
                    integrityCheckResult.AddFailureMessage($"The provided json contains invalid TargetMapping, '{element.Properties.QuestionId}' contains invalid characters");
                }
            }
            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}