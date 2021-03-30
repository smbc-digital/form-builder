using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using form_builder.Models;
using form_builder.Extensions;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public class CurrentEnvironmentSubmitSlugsCheck : IBehaviourSchemaIntegrityCheck
    {
        readonly IWebHostEnvironment _environment;
        public CurrentEnvironmentSubmitSlugsCheck(IWebHostEnvironment environment) => 
            _environment= environment;

        public IntegrityCheckResult Validate(List<Behaviour> behaviours)
        {
            IntegrityCheckResult result = new();

            foreach (var behaviour in behaviours)
            {
                if (behaviour.SubmitSlugs is not null && 
                    !behaviour.SubmitSlugs
                    .Any(submitSlug => submitSlug.Environment
                    .Equals(_environment.EnvironmentName.ToS3EnvPrefix(), StringComparison.OrdinalIgnoreCase)))
                {
                    result.AddFailureMessage($"No SubmitSlug found for environment '{_environment.EnvironmentName}'.");
                }
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(List<Behaviour> behaviours) => await Task.Run(() => Validate(behaviours));
    }
}