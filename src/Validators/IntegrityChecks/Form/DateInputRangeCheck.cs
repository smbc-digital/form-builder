using System;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;

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
                if (!element.Properties.OutsideRangeType.Equals(DateInputConstants.YEAR) && !element.Properties.OutsideRangeType.Equals(DateInputConstants.MONTH) && !element.Properties.OutsideRangeType.Equals(DateInputConstants.DAY))
                    result.AddFailureMessage($"The provided json has date input element with a incorrect outside range value for '{ String.Join(", ", element.Properties.QuestionId) }'");
            }

            foreach (var element in dateInputWithinRangeElements)
            {
                if (!element.Properties.WithinRangeType.Equals(DateInputConstants.YEAR) && !element.Properties.WithinRangeType.Equals(DateInputConstants.MONTH) && !element.Properties.WithinRangeType.Equals(DateInputConstants.DAY))
                    result.AddFailureMessage($"The provided json has date input element with a incorrect within range value for '{ String.Join(", ", element.Properties.QuestionId) }'");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
