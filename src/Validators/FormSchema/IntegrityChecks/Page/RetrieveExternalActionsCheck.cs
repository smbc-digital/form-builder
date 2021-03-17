using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Validators.IntegrityChecks.Page
{
    public class RetrieveExternalActionsCheck: IPageSchemaIntegrityCheck
    {
        private readonly IWebHostEnvironment _environment;

        public RetrieveExternalActionsCheck(IWebHostEnvironment enviroment)
        {
            _environment = enviroment;
        }

        public IntegrityCheckResult Validate(Models.Page page)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            var actions = schema.FormActions.Where(formAction => formAction.Type.Equals(EActionType.RetrieveExternalData))
                .Concat(schema.Pages.SelectMany(page => page.PageActions)
                .Where(pageAction => pageAction.Type == EActionType.RetrieveExternalData)).ToList();

            if (!actions.Any())
                return IntegrityCheckResult.ValidResult;

            actions.ForEach(action =>
            {
                var slug = action.Properties.PageActionSlugs.FirstOrDefault(slugs => slugs.Environment.ToLower().Equals(_environment.EnvironmentName.ToS3EnvPrefix().ToLower()));
                if (slug == null)
                {
                    integrityCheckResult.AddFailureMessage($"Retrieve External Data Action, there is no PageActionSlug for environment '{_environment.EnvironmentName}'");
                }
                else{
                    if (string.IsNullOrEmpty(slug.URL))
                        integrityCheckResult.AddFailureMessage("Retrieve External Data Action, action type does not contain a url");
                }

                if (string.IsNullOrEmpty(action.Properties.TargetQuestionId))
                    integrityCheckResult.AddFailureMessage("Retrieve External Data Action, action type does not contain a TargetQuestionId");
            });

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Models.Page page) => await Task.Run(() => Validate(schema));
    }
}