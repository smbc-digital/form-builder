using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public class CurrentEnvironmentSubmitSlugsCheck: IBehaviourSchemaIntegrityCheck
    {
        IWebHostEnvironment _environment;
        public CurrentEnvironmentSubmitSlugsCheck(IWebHostEnvironment environment)
        {
            _environment= environment;
        }

        public IntegrityCheckResult Validate(Behaviour behaviour)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            var foundEnvironmentSubmitSlug = false;
            foreach (var subItem in behaviour.SubmitSlugs.Where(subItem => subItem.Environment.ToLower().Equals(_environment.EnvironmentName.ToS3EnvPrefix().ToLower())))
                foundEnvironmentSubmitSlug = true;

            if (!foundEnvironmentSubmitSlug)
                integrityCheckResult.AddFailureMessage($"No SubmitSlug found in form '{schema.FormName}' for environment '{_environment.EnvironmentName}'.");

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Behaviour behaviour) => await Task.Run(() => Validate(behaviour));
    }
}