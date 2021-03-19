using System.Threading.Tasks;
using System.Text.RegularExpressions;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class InvalidQuestionOrTargetMappingValueCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (element.Properties is null)
                return result;

            Regex regex = new Regex(@"^[a-zA-Z.]+$", RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(element.Properties.QuestionId) &&
                !regex.IsMatch(element.Properties.QuestionId))
                result.AddFailureMessage($"The provided json contains invalid QuestionIDs, '{element.Properties.QuestionId}' contains invalid characters");

            if (!string.IsNullOrEmpty(element.Properties.TargetMapping) && 
                !regex.IsMatch(element.Properties.TargetMapping) || 
                element.Properties.TargetMapping.EndsWith(".") || 
                element.Properties.TargetMapping.StartsWith("."))
                result.AddFailureMessage($"The provided json contains invalid TargetMapping, '{element.Properties.QuestionId}' contains invalid characters");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}