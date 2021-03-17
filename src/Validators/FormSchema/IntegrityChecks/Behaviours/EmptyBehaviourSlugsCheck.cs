using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Behaviours
{
    public class EmptyBehaviourSlugsCheck: IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            var behaviours = new List<Behaviour>();
            var pagesWithBehaviours = schema.Pages.Where(page => page.Behaviours != null);
            foreach (var page in pagesWithBehaviours)
            {
                behaviours.AddRange(page.Behaviours);
            }

            if (behaviours.Any(item => string.IsNullOrEmpty(item.PageSlug) && (item.SubmitSlugs == null || item.SubmitSlugs.Count == 0)))
                integrityCheckResult.AddFailureMessage($"Empty Behaviour Slugs Check, Incorrectly configured behaviour slug was discovered in '{schema.FormName}' form");

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}