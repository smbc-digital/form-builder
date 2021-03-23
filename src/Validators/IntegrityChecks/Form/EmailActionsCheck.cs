using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Actions;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class EmailActionsCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            List<IAction> userEmail = schema.FormActions
                .Where(formAction => formAction.Type.Equals(EActionType.UserEmail))
                .Concat(schema.Pages.SelectMany(page => page.PageActions)
                .Where(pageAction => pageAction.Type.Equals(EActionType.UserEmail)))
                .ToList();

            List<IAction> backOfficeEmail = schema.FormActions
                .Where(formAction => formAction.Type.Equals(EActionType.BackOfficeEmail))
                .Concat(schema.Pages.SelectMany(page => page.PageActions)
                .Where(pageAction => pageAction.Type.Equals(EActionType.BackOfficeEmail)))
                .ToList();

            List<IAction> actions = userEmail
                .Concat(backOfficeEmail)
                .ToList();

            if (actions.Count == 0)
                return result;

            actions.ForEach(action =>
            {
                if (string.IsNullOrEmpty(action.Properties.Content))
                    result.AddFailureMessage($"Email Actions Check, Content doesn't have a value");

                if (string.IsNullOrEmpty(action.Properties.To))
                    result.AddFailureMessage("Email Actions Check, To doesn't have a value");

                if (string.IsNullOrEmpty(action.Properties.From))
                    result.AddFailureMessage("Email Actions Check, From doesn't have a value");

                if (string.IsNullOrEmpty(action.Properties.Subject))
                    result.AddFailureMessage("Email Actions Check, Subject doesn't have a value");
            });

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
