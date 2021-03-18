using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public class EmptyBehaviourSlugsCheck: IBehaviourSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(List<Behaviour> behaviours, string formName)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            foreach (var behaviour in behaviours)
            {
                if (string.IsNullOrEmpty(behaviour.PageSlug) && (behaviour.SubmitSlugs == null || behaviour.SubmitSlugs.Count == 0))
                    integrityCheckResult.AddFailureMessage($"Empty Behaviour Slugs Check, Incorrectly configured behaviour slug was discovered in '{schema.FormName}' form");
            }
            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(List<Behaviour> behaviours, string formName) => await Task.Run(() => Validate(behaviour, formName));
    }
}