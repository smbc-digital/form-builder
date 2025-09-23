using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class TextboxElementCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (!element.Type.Equals(EElementType.Textbox))
                return result;

            if (element.Properties is null)
                return result;

            if (element.Properties.Decimal && element.Properties.Numeric)
                result.AddFailureMessage(
                    $"Textbox element {element.Properties.QuestionId} cannot have both Decimal and Numeric set to 'true', " +
                    "only one is allowed.");

            if (element.Properties.MinLength is not null && element.Properties.MinLength < 0)
                result.AddFailureMessage(
                    $"Textbox element {element.Properties.QuestionId} cannot have a minLength of less than 0, " +
                    "increase the minLength.");

            if (element.Properties.Optional && element.Properties.MinLength is not null)
                result.AddFailureMessage(
                    $"Textbox element {element.Properties.QuestionId} cannot have a minLength value and Optional set to 'true', " +
                    "remove the minLength or change Optional to 'false'.");

            if (!element.Properties.Optional && element.Properties.MinLength is 0)
                result.AddFailureMessage(
                    $"Textbox element {element.Properties.QuestionId} cannot have have a minLength of 0 and Optional set to 'false', " +
                    "increase the minLength or change Optional to 'true'.");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}
