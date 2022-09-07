using System.Text.RegularExpressions;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class InvalidQuestionCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (element.Properties is null || string.IsNullOrEmpty(element.Properties.QuestionId))
                return result;

            Regex regex = new(@"^[a-zA-Z]+$", RegexOptions.IgnoreCase);

            if (!regex.IsMatch(element.Properties.QuestionId))
            {
                result.AddFailureMessage(
                    "The provided json contains invalid QuestionIDs, " +
                    $"'{element.Properties.QuestionId}' contains invalid characters");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}