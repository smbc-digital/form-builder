using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using form_builder.Extensions;
using form_builder.Models;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public class CurrentEnvironmentSubmitSlugsCheck : IBehaviourSchemaIntegrityCheck
    {
        IWebHostEnvironment _environment;
        public CurrentEnvironmentSubmitSlugsCheck(IWebHostEnvironment environment) => _environment= environment;

        public IntegrityCheckResult Validate(List<Behaviour> behaviours)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            var foundEnvironmentSubmitSlug = false;

            foreach (var behaviour in behaviours)
            {
                foreach (var subItem in behaviour.SubmitSlugs.Where(subItem => subItem.Environment.Equals(_environment.EnvironmentName.ToS3EnvPrefix(), StringComparison.OrdinalIgnoreCase)))
                    foundEnvironmentSubmitSlug = true;
            }

            if (!foundEnvironmentSubmitSlug)
                integrityCheckResult.AddFailureMessage($"No SubmitSlug found for environment '{_environment.EnvironmentName}'.");

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(List<Behaviour> behaviours) => await Task.Run(() => Validate(behaviours));
    }
}