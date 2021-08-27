using form_builder.Enum;
using form_builder.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class DateInputRangeCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            if (!schema.Pages.Any(page => page.Elements.Any(element => element.Type.Equals(EElementType.DateInput))))
                return result;

            var pagesWithElements = schema.Pages
                .Where(page => page.Elements is not null);

            var dateInputElements = pagesWithElements
                .SelectMany(page => page.Elements
                    .Where(element => element.Type.Equals(EElementType.DateInput)));

            foreach (var element in dateInputElements)
            {
                foreach (var outsideRange in element.Properties.OutsideRangeType)
                {
                    if (!outsideRange.Equals('Y') && !outsideRange.Equals('M') && !outsideRange.Equals('D'))
                        result.AddFailureMessage($"The provided json has Dateinput elements with a incorrect range format for '{ String.Join(", ", element.Properties.QuestionId) }'");
                }

                foreach (var withinRange in element.Properties.WithinRangeType)
                {
                    if (!withinRange.Equals('Y') && !withinRange.Equals('M') && !withinRange.Equals('D'))
                        result.AddFailureMessage($"The provided json has Dateinput elements with a incorrect range format for '{ String.Join(", ", element.Properties.QuestionId) }'");
                }
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
