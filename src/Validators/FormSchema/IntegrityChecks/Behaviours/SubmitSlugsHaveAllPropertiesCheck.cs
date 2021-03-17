using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public class SubmitSlugsHaveAllPropertiesCheck: IFormSchemaIntegrityCheck
    {
        IWebHostEnvironment _environment;

        public SubmitSlugsHaveAllPropertiesCheck(IWebHostEnvironment environment)
        {
            _environment= environment;
        }
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            IEnumerable<Behaviour> behaviours = schema.Pages.Where(page => page.Behaviours != null).SelectMany(page => page.Behaviours).ToList();
            
            foreach (var item in behaviours)
            {
                if (item.BehaviourType != EBehaviourType.SubmitForm && item.BehaviourType != EBehaviourType.SubmitAndPay) continue;

                if (item.SubmitSlugs.Count <= 0) continue;

                foreach (var submitSlug in item.SubmitSlugs)
                {
                    if (string.IsNullOrEmpty(submitSlug.URL))
                        integrityCheckResult.AddFailureMessage($"No URL found for SubmitSlug in environmment '{submitSlug.Environment}' in form '{schema.FormName}'");

                    if (string.IsNullOrEmpty(submitSlug.AuthToken))
                        integrityCheckResult.AddFailureMessage($"No auth token found for SubmitSlug in environmment '{submitSlug.Environment}' in form '{schema.FormName}'");

                    if (!_environment.IsEnvironment("local") && !submitSlug.Environment.ToLower().Equals("local") && !submitSlug.URL.StartsWith("https://"))
                        integrityCheckResult.AddFailureMessage($"SubmitUrl '{submitSlug.URL}' must start with https for environmment '{submitSlug.Environment}'");
                }
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}