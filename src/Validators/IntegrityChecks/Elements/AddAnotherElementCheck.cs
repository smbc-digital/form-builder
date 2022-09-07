using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class AddAnotherElementCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (!element.Type.Equals(EElementType.AddAnother))
                return result;

            if (element.Properties is null)
            {
                result.AddFailureMessage("Add Another Element Check : No Properties section found for this element.");
                return result;
            }

            if (element.Properties.Elements is null)
            {
                result.AddFailureMessage("Add Another Element Check : No Elements section found for this element.");
                return result;
            }

            var nestedElements = element.Properties.Elements;
            if (nestedElements.Count.Equals(0))
            {
                result.AddFailureMessage("Add Another Element Check : No elements found.");
                return result;
            }

            return result;
        }
        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}
