using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class RenderConditionsValidCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            var groups = schema.Pages
                .GroupBy(page => page.PageSlug, (key, g) => new { Slug = key, Pages = g.ToList() });

            foreach (var group in groups)
            {
                if (group.Pages.Count(page => !page.HasRenderConditions) > 1)
                    result.AddFailureMessage($"Render Conditions Valid Check, More than one {group.Slug} page has no render conditions");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
