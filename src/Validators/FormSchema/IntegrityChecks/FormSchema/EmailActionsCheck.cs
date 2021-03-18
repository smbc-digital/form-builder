using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;

namespace form_builder.Validators.IntegrityChecks.FormSchema
{
    public class EmailActionsCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(Models.FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            var userEmail = schema.FormActions.Where(_ => _.Type.Equals(EActionType.UserEmail))
                .Concat(schema.Pages.SelectMany(_ => _.PageActions)
                .Where(_ => _.Type == EActionType.UserEmail)).ToList();

            var backOfficeEmail = schema.FormActions.Where(_ => _.Type.Equals(EActionType.BackOfficeEmail))
                .Concat(schema.Pages.SelectMany(_ => _.PageActions)
                .Where(_ => _.Type == EActionType.BackOfficeEmail)).ToList();

            var actions = userEmail.Concat(backOfficeEmail).ToList();

            if (actions.Any())
            {
                actions.ForEach(action =>
                {
                    if (string.IsNullOrEmpty(action.Properties.Content))
                        integrityCheckResult.AddFailureMessage($"Email Actions Check, Content doesn't have a value");

                    if (string.IsNullOrEmpty(action.Properties.To))
                        integrityCheckResult.AddFailureMessage("Email Actions Check, To doesn't have a value");

                    if (string.IsNullOrEmpty(action.Properties.From))
                        integrityCheckResult.AddFailureMessage("Email Actions Check, From doesn't have a value");

                    if (string.IsNullOrEmpty(action.Properties.Subject))
                        integrityCheckResult.AddFailureMessage("Email Actions Check, Subject doesn't have a value");
                });
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}