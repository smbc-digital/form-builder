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

            if (!schema.GenerateReferenceNumber &&
                schema.Pages is not null &&
                schema.Pages.Any(page => page.Behaviours is not null &&
                                         page.Behaviours.Any(behaviour => behaviour.BehaviourType.Equals(EBehaviourType.SubmitWithoutSubmission))))
            {
                result.AddFailureMessage(
                    "Generated Id Configuration Check, if using SubmitFormNoAction then 'GeneratedReferenceNumber' must be true.");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
