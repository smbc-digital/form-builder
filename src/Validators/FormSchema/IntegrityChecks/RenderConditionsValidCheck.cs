using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks
{
    public class RenderConditionsValidCheck: IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            var groups = schema.Pages.GroupBy(page => page.PageSlug, (key, g) => new { Slug = key, Pages = g.ToList() });
            foreach (var group in groups)
            {
                if (group.Pages.Count(page => !page.HasRenderConditions) > 1)
                    integrityCheckResult.AddFailureMessage($"Render Conditions Valid Check, More than one {@group.Slug} page has no render conditions");
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}