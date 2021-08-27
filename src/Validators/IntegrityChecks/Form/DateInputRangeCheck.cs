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

            var dateInputOutsideRangeElements = pagesWithElements
                .SelectMany(page => page.Elements
                    .Where(element => element.Type.Equals(EElementType.DateInput) && !string.IsNullOrEmpty(element.Properties.OutsideRangeType)));

            var dateInputWithinRangeElements = pagesWithElements
                .SelectMany(page => page.Elements
                    .Where(element => element.Type.Equals(EElementType.DateInput) && !string.IsNullOrEmpty(element.Properties.WithinRangeType)));

            foreach (var element in dateInputOutsideRangeElements)
            {
                if (!element.Properties.OutsideRangeType.Equals("Y") && !element.Properties.OutsideRangeType.Equals("M") && !element.Properties.OutsideRangeType.Equals("D"))
                    result.AddFailureMessage($"The provided json has Dateinput elements with a incorrect range format for '{ String.Join(", ", element.Properties.QuestionId) }'");
            }

            foreach (var element in dateInputWithinRangeElements)
            {
                if (!element.Properties.WithinRangeType.Equals("Y") && !element.Properties.WithinRangeType.Equals("M") && !element.Properties.WithinRangeType.Equals("D"))
                    result.AddFailureMessage($"The provided json has Dateinput elements with a incorrect range format for '{ String.Join(", ", element.Properties.QuestionId) }'");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
