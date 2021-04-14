using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class SummaryElementFormCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            if (!schema.Pages.Any(page => page.Elements.Any(element => element.Type.Equals(EElementType.Summary))))
                return result;

            var summaryElements = schema.Pages.SelectMany(page => page.Elements.Where(element => element.Type.Equals(EElementType.Summary))).ToList();


            summaryElements.ForEach((element) =>
            {
                var properties = element.Properties;
                if (properties.HasSummarySectionsDefined)
                {
                    properties.Sections.ForEach(_ =>
                    {
                        if (string.IsNullOrEmpty(_.Title))
                        {
                            result.AddFailureMessage($"FormSchemaIntegrityCheck::SummaryFormCheck, Summary section is defined but section title is empty. Please add a title for the section.");
                        }

                        if (_.Pages is null || _.Pages.Count.Equals(0))
                        {
                            result.AddFailureMessage($"FormSchemaIntegrityCheck::SummaryFormCheck, Summary section is defined but no pages have been specified to appear in the section.");
                        }

                        if (_.Pages is not null && _.Pages.Count > 0)
                        {
                            _.Pages.ForEach((x) =>
                            {
                                if (!schema.Pages.Any(_ => _.PageSlug.Equals(x)))
                                {
                                    result.AddFailureMessage($"FormSchemaIntegrityCheck::SummaryFormCheck, Summary section has a page slug defined which cannot be found. Verify '{x}' is a valid page slug within the schema.");
                                }
                            });
                        }
                    });
                }
            });

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
