using System.Text.RegularExpressions;
using System.Threading.Tasks;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class InvalidTargetMappingValueCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (element.Properties is null || string.IsNullOrEmpty(element.Properties.TargetMapping))
                return result;

            Regex regex = new(@"^[a-zA-Z.]+$", RegexOptions.IgnoreCase);

            if (!regex.IsMatch(element.Properties.TargetMapping) ||
                element.Properties.TargetMapping.EndsWith(".") ||
                element.Properties.TargetMapping.StartsWith("."))
                    result.AddFailureMessage($"The provided json contains invalid TargetMapping, '{element.Properties.QuestionId}' contains invalid characters");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}