using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Models.Properties.ActionProperties;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class ValidateActionCheck : IFormSchemaIntegrityCheck
    {
        private readonly IWebHostEnvironment _environment;

        public ValidateActionCheck(IWebHostEnvironment enviroment) =>
            _environment = enviroment;

        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            List<IAction> actions = schema.FormActions
                .Where(formAction => formAction.Type.Equals(EActionType.Validate))
                .Concat(schema.Pages.SelectMany(page => page.PageActions)
                .Where(action => action.Type.Equals(EActionType.Validate)))
                .ToList();

            if (actions.Count.Equals(0))
                return result;

            actions.ForEach(action =>
            {
                PageActionSlug slug = action.Properties.PageActionSlugs
                    .FirstOrDefault(slug => slug.Environment
                    .Equals(_environment.EnvironmentName.ToS3EnvPrefix(), StringComparison.OrdinalIgnoreCase));

                if (slug is null)
                    result.AddFailureMessage($"Validate Action Check, Validate there is no PageActionSlug for {_environment.EnvironmentName}");

                if (string.IsNullOrEmpty(slug.URL))
                    result.AddFailureMessage("Validate Action Check, Validate action type does not contain a url");

                if (action.Properties.HttpActionType.Equals(EHttpActionType.Unknown))
                    result.AddFailureMessage("Validate Action Check, Validate action type does not contain 'Unknown'");
            });

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
