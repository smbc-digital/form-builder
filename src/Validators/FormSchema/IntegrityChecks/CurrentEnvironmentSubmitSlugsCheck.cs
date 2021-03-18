using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Validators.IntegrityChecks
{
    public class CurrentEnvironmentSubmitSlugsCheck: IFormSchemaIntegrityCheck
    {
        IWebHostEnvironment _environment;
        public CurrentEnvironmentSubmitSlugsCheck(IWebHostEnvironment environment)
        {
            _environment= environment;
        }

        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            var behaviours = schema.Pages.Where(page => page.Behaviours != null).SelectMany(page => page.Behaviours).ToList();
            
            foreach (var item in behaviours)
            {
                if (item.BehaviourType != EBehaviourType.SubmitForm && item.BehaviourType != EBehaviourType.SubmitAndPay) continue;

                if (item.SubmitSlugs.Count <= 0) continue;

                var foundEnvironmentSubmitSlug = false;
                foreach (var subItem in item.SubmitSlugs.Where(subItem => subItem.Environment.ToLower().Equals(_environment.EnvironmentName.ToS3EnvPrefix().ToLower())))
                    foundEnvironmentSubmitSlug = true;

                if (!foundEnvironmentSubmitSlug)
                    integrityCheckResult.AddFailureMessage($"No SubmitSlug found in form '{schema.FormName}' for environment '{_environment.EnvironmentName}'.");
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}