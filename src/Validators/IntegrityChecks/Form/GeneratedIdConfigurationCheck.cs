using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class GeneratedIdConfigurationCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            if (schema.GenerateReferenceNumber && (string.IsNullOrEmpty(schema.GeneratedReferenceNumberMapping) ||
                                                   string.IsNullOrEmpty(schema.ReferencePrefix)))
                result.AddFailureMessage(
                    "Generated Id Configuration Check, " +
                    "'GeneratedReferenceNumberMapping' and 'ReferencePrefix' must both have a value.");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
