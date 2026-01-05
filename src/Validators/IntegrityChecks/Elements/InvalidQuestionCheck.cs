using System.Text.RegularExpressions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class InvalidQuestionCheck(IElementHelper elementHelper) : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (elementHelper.IsElementANonInputType(element))
                return result;

            if (element.Properties is null)
            {
                result.AddFailureMessage(
                    "The provided json contains invalid Properties, " +
                    $"'{element.Type}' must have Properties");

                return result;
            }

            if (string.IsNullOrEmpty(element.Properties.QuestionId))
            {
                result.AddFailureMessage(
                    "The provided json contains invalid QuestionIds, " +
                    $"'{element.Type}' must have a QuestionId");

                return result;
            }

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