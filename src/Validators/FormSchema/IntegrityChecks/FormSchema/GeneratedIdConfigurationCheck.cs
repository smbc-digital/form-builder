using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.FormSchema
{
    public class GeneratedIdConfigurationCheck: IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(Models.FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            if (schema.GenerateReferenceNumber)
            {
                if (string.IsNullOrEmpty(schema.GeneratedReferenceNumberMapping) || string.IsNullOrEmpty(schema.ReferencePrefix))
                    integrityCheckResult.AddFailureMessage($"Generated Id Configuration Check, 'GeneratedReferenceNumberMapping' and 'ReferencePrefix' must both have a value in form {schema.FormName}");
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}