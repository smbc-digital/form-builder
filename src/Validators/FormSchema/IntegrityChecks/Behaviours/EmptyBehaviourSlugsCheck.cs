using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public class EmptyBehaviourSlugsCheck: IBehaviourSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(Behaviour behaviour)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            if (string.IsNullOrEmpty(behaviour.PageSlug) && (behaviour.SubmitSlugs == null || behaviour.SubmitSlugs.Count == 0))
                integrityCheckResult.AddFailureMessage($"Empty Behaviour Slugs Check, Incorrectly configured behaviour slug was discovered in '{schema.FormName}' form");

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Behaviour behaviour) => await Task.Run(() => Validate(behaviour));
    }
}