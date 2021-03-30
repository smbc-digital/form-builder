using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using form_builder.Models;
using System;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public class SubmitSlugsHaveAllPropertiesCheck : IBehaviourSchemaIntegrityCheck
    {
        readonly IWebHostEnvironment _environment;
        public SubmitSlugsHaveAllPropertiesCheck(IWebHostEnvironment environment) =>
            _environment = environment;

        public IntegrityCheckResult Validate(List<Behaviour> behaviours)
        {
            IntegrityCheckResult result = new();

            foreach (var behaviour in behaviours.Where(behaviour => behaviour.SubmitSlugs is not null && behaviour.SubmitSlugs.Count > 0))
            {
                foreach (var submitSlug in behaviour.SubmitSlugs)
                {
                    if (string.IsNullOrEmpty(submitSlug.URL))
                        result.AddFailureMessage($"No URL found for SubmitSlug in environmment '{submitSlug.Environment}'");

                    if (string.IsNullOrEmpty(submitSlug.AuthToken))
                        result.AddFailureMessage($"No auth token found for SubmitSlug in environmment '{submitSlug.Environment}'");

                    if (!_environment.IsEnvironment("local") && !submitSlug.Environment.Equals("local", StringComparison.OrdinalIgnoreCase) && !submitSlug.URL.StartsWith("https://"))
                        result.AddFailureMessage($"SubmitUrl '{submitSlug.URL}' must start with https for environmment '{submitSlug.Environment}'");
                }
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(List<Behaviour> behaviours) => await Task.Run(() => Validate(behaviours));
    }
}
