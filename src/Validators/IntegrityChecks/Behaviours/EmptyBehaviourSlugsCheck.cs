using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public class EmptyBehaviourSlugsCheck: IBehaviourSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(List<Behaviour> behaviours)
        {
            IntegrityCheckResult result = new();

            foreach (var behaviour in behaviours)
            {
                if (string.IsNullOrEmpty(behaviour.PageSlug) && (behaviour.SubmitSlugs is null || behaviour.SubmitSlugs.Count == 0))
                    result.AddFailureMessage("Empty Behaviour Slug, Incorrectly configured behaviour slug was discovered");
            }
            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(List<Behaviour> behaviours) => await Task.Run(() => Validate(behaviours));
    }
}