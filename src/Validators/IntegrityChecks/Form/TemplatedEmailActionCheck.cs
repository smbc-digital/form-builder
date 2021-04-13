using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Actions;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                string provider = action.Properties.EmailTemplateProvider;

                if (string.IsNullOrEmpty(provider))
                    result.AddFailureMessage("Templated Email Action, there is no 'EmailTemplateProvider'");

                if (string.IsNullOrEmpty(action.Properties.TemplateId))
                    result.AddFailureMessage("Templated Email Action, there is no 'TemplateId' provided");

                if (action.Properties.Personlisation is null || action.Properties.Personlisation.Count.Equals(0))
                    result.AddFailureMessage("Templated Email Action, there is no 'Personlisation' field(s) added");
            });

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
