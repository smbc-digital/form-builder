using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class PaymentCallbackCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            if (schema.ProcessPaymentCallbackResponse && string.IsNullOrEmpty(schema.CallbackFailureContactNumber))
                result.AddFailureMessage("Callback Failure Contact Number Valid Check, CallbackFailureContactNumber cannot be null or empty if ProcessPaymentCallbackResponse is true");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
