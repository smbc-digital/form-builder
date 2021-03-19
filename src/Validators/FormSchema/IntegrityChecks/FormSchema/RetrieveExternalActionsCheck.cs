using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Validators.IntegrityChecks.FormSchema
{
    public class RetrieveExternalActionsCheck : IFormSchemaIntegrityCheck
    {
        private readonly IWebHostEnvironment _environment;

        public RetrieveExternalActionsCheck(IWebHostEnvironment enviroment) => _environment = enviroment;

        public IntegrityCheckResult Validate(Models.FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            var actions = schema.FormActions
                .Where(formAction => formAction.Type.Equals(EActionType.RetrieveExternalData))
                .Concat(schema.Pages.SelectMany(page => page.PageActions)
                .Where(pageAction => pageAction.Type == EActionType.RetrieveExternalData)).ToList();

            if (!actions.Any())
                return IntegrityCheckResult.ValidResult;

            actions.ForEach(action =>
            {
                PageActionSlug slug = action.Properties.PageActionSlugs.FirstOrDefault(slugs => slugs.Environment.ToLower().Equals(_environment.EnvironmentName.ToS3EnvPrefix().ToLower()));

                if (slug == null)
                {
                    integrityCheckResult.AddFailureMessage($"Retrieve External Data Action, there is no PageActionSlug for environment '{_environment.EnvironmentName}'");
                }
                else
                {
                    if (string.IsNullOrEmpty(slug.URL))
                        integrityCheckResult.AddFailureMessage("Retrieve External Data Action, action type does not contain a url");
                }

                if (string.IsNullOrEmpty(action.Properties.TargetQuestionId))
                    integrityCheckResult.AddFailureMessage("Retrieve External Data Action, action type does not contain a TargetQuestionId");
            });

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
