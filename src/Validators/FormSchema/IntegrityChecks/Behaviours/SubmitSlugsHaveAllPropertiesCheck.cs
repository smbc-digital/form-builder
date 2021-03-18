using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public class SubmitSlugsHaveAllPropertiesCheck : IBehaviourSchemaIntegrityCheck
    {
        IWebHostEnvironment _environment;
        public SubmitSlugsHaveAllPropertiesCheck(IWebHostEnvironment environment) => _environment = environment;

        public IntegrityCheckResult Validate(List<Behaviour> behaviours, string formName)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            foreach (var behaviour in behaviours.Where(behaviour => behaviour.SubmitSlugs.Any()))
            {
                foreach (var submitSlug in behaviour.SubmitSlugs)
                {
                    if (string.IsNullOrEmpty(submitSlug.URL))
                        integrityCheckResult.AddFailureMessage($"No URL found for SubmitSlug in environmment '{submitSlug.Environment}' in form '{formName}'");

                    if (string.IsNullOrEmpty(submitSlug.AuthToken))
                        integrityCheckResult.AddFailureMessage($"No auth token found for SubmitSlug in environmment '{submitSlug.Environment}' in form '{formName}'");

                    if (!_environment.IsEnvironment("local") && !submitSlug.Environment.ToLower().Equals("local") && !submitSlug.URL.StartsWith("https://"))
                        integrityCheckResult.AddFailureMessage($"SubmitUrl '{submitSlug.URL}' must start with https for environmment '{submitSlug.Environment}'");
                }
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(List<Behaviour> behaviours, string formName) => await Task.Run(() => Validate(behaviours, formName));
    }
}