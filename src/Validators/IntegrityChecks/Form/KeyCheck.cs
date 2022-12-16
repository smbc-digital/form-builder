using form_builder.Constants;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class KeyCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            if (!string.IsNullOrEmpty(schema.Key))
            {
                if(string.IsNullOrEmpty(schema.KeyName))
                    result.AddFailureMessage("FormSchema Key Check, 'KeyName' can not be empty if 'Key' has been specified");

                return result;
            }

            if (!string.IsNullOrEmpty(schema.KeyName))
            {
                if(string.IsNullOrEmpty(schema.Key))
                    result.AddFailureMessage("FormSchema Key Check, 'Key' can not be empty if a 'KeyName' has been specified");

                return result;
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
