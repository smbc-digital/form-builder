using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Validators.IntegrityChecks.FormSchema;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks
{
    public class ValidateActionCheck : IFormSchemaIntegrityCheck
    {
        private readonly IWebHostEnvironment _environment;

        public ValidateActionCheck(IWebHostEnvironment enviroment) =>
            _environment = enviroment;

        public IntegrityCheckResult Validate(Models.FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            List<IAction> actions = schema.FormActions
                .Where(formAction => formAction.Type.Equals(EActionType.Validate))
                .Concat(schema.Pages.SelectMany(page => page.PageActions)
                .Where(action => action.Type == EActionType.Validate))
                .ToList();

            if (!actions.Any())
                return IntegrityCheckResult.ValidResult;

            actions.ForEach(action =>
            {
                PageActionSlug slug = action.Properties.PageActionSlugs.FirstOrDefault(slug => slug.Environment.ToLower().Equals(_environment.EnvironmentName.ToS3EnvPrefix().ToLower()));

                if (slug == null)
                    integrityCheckResult.AddFailureMessage($"Validate Action Check, Validate there is no PageActionSlug for {_environment.EnvironmentName}");

                if (string.IsNullOrEmpty(slug.URL))
                    integrityCheckResult.AddFailureMessage("Validate Action Check, Validate action type does not contain a url");

                if (action.Properties.HttpActionType == EHttpActionType.Unknown)
                    integrityCheckResult.AddFailureMessage("Validate Action Check, Validate action type does not contain 'Unknown'");
            });

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
