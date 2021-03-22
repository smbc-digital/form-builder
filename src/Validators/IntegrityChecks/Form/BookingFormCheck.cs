using System;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Constants;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class BookingFormCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            if (!schema.Pages.Any(page => page.Elements.Any(element => element.Type.Equals(EElementType.Booking))))
                return result;

            if (!schema.Pages.Any(page => page.PageSlug.Equals(BookingConstants.NO_APPOINTMENT_AVAILABLE, StringComparison.OrdinalIgnoreCase)))
                result.AddFailureMessage($"Booking Element Check, Form contains booking element, but is missing required page with slug {BookingConstants.NO_APPOINTMENT_AVAILABLE}.");

            var requiredFields = new[] { "customer.firstname", "customer.lastname" };
            foreach (var requiredField in requiredFields)
            {
                if (!schema.Pages.Any(page => page.Elements.Any(element => 
                    element.Properties is not null &&
                    element.Properties.TargetMapping is not null &&
                    element.Properties.TargetMapping.Equals(requiredField, StringComparison.OrdinalIgnoreCase))))
                        result.AddFailureMessage($"Booking Element Check, Booking element requires {requiredField} elements for reservation.");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
