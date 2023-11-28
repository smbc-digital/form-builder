using System.Text.RegularExpressions;
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

            var relativeDateStringRegex = new Regex(@"^\d+-[dmy]-(ex|in)$");

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

            if (!string.IsNullOrEmpty(element.Properties.IsFutureDateAfterRelative) &&
                !relativeDateStringRegex.Match(element.Properties.IsFutureDateAfterRelative.ToLower()).Success)
            {
                result.AddFailureMessage("Property 'IsPastDateBeforeRelative' is invalid");
            }

            if (!string.IsNullOrEmpty(element.Properties.IsFutureDateBeforeRelative) &&
                !relativeDateStringRegex.Match(element.Properties.IsFutureDateBeforeRelative.ToLower()).Success)
            {
                result.AddFailureMessage("Property 'IsPastDateBeforeRelative' is invalid");
            }

            if (!string.IsNullOrEmpty(element.Properties.IsPastDateBeforeRelative) &&
                !relativeDateStringRegex.Match(element.Properties.IsPastDateBeforeRelative.ToLower()).Success)
            {
                result.AddFailureMessage("Property 'IsPastDateBeforeRelative' is invalid");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}