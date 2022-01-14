using System.Threading.Tasks;
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
            {
                result.AddFailureMessage(
                    $"Textbox element {element.Properties.QuestionId} cannot have both Decimal and Numeric set to 'true', " +
                    "only one is allowed.");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}
