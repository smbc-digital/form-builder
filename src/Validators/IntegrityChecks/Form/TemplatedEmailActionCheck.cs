using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Actions;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class TemplatedEmailActionCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            List<IAction> actions = schema.FormActions
                .Where(formAction => formAction.Type.Equals(EActionType.TemplatedEmail))
                .Concat(schema.Pages.SelectMany(page => page.PageActions)
                    .Where(pageAction => pageAction.Type.Equals(EActionType.TemplatedEmail))).ToList();

            if (actions.Count == 0)
                return result;

            actions.ForEach(action =>
            {
                if (string.IsNullOrEmpty(action.Properties.EmailTemplateProvider))
                    result.AddFailureMessage("Templated Email Action, there is no 'EmailTemplateProvider'");

                if (string.IsNullOrEmpty(action.Properties.TemplateId))
                    result.AddFailureMessage("Templated Email Action, there is no 'TemplateId' provided");

                if (string.IsNullOrEmpty(action.Properties.To))
                    result.AddFailureMessage("Templated Email Action, there is no 'To' provided");
            });

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
