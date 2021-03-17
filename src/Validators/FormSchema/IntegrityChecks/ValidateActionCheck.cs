using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Validators.IntegrityChecks
{
    public class ValidateActionCheck: IFormSchemaIntegrityCheck
    {
        private readonly IWebHostEnvironment _environment;

        public ValidateActionCheck(IWebHostEnvironment enviroment)
        {
            _environment = enviroment;
        }

        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            var actions = schema.FormActions.Where(formAction => formAction.Type.Equals(EActionType.Validate))
                .Concat(schema.Pages.SelectMany(page => page.PageActions)
                .Where(action => action.Type == EActionType.Validate)).ToList();

            if (!actions.Any())
                return IntegrityCheckResult.ValidResult;

            actions.ForEach(action =>
            {
                var slug = action.Properties.PageActionSlugs.FirstOrDefault(slug => slug.Environment.ToLower().Equals(_environment.EnvironmentName.ToS3EnvPrefix().ToLower()));
                if (slug == null)
                    integrityCheckResult.AddFailureMessage($"Validate Action Check, Validate there is no PageActionSlug for {_environment.EnvironmentName}");

                if (string.IsNullOrEmpty(slug.URL))
                    integrityCheckResult.AddFailureMessage("Validate Action Check, Validate action type does not contain a url");

                if (action.Properties.HttpActionType == EHttpActionType.Unknown)
                    integrityCheckResult.AddFailureMessage("Validate Action Check, Validate action type does not contain 'Unknown'");
            });

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}