using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Constants;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class BookingFormCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            List<IElement> bookingElements = schema.Pages
                 .SelectMany(page => page.Elements)
                 .Where(element => element.Type.Equals(EElementType.Booking))
                 .ToList();

            if (bookingElements.Count == 0)
                return result;

            if (!schema.Pages.Any(page => page.PageSlug.Equals(BookingConstants.NO_APPOINTMENT_AVAILABLE, StringComparison.OrdinalIgnoreCase)))
                result.AddFailureMessage($"Booking Element Check, Form contains booking element, but is missing required page with slug {BookingConstants.NO_APPOINTMENT_AVAILABLE}.");

            var requiredFields = new[] { "customer.firstname", "customer.lastname" };
            foreach (var requiredField in requiredFields)
            {
                if (!bookingElements.Any(
                    element => element.Properties is not null &&
                    element.Properties.TargetMapping is not null &&
                    element.Properties.TargetMapping.Equals(requiredField, StringComparison.OrdinalIgnoreCase)))
                    result.AddFailureMessage($"Booking Element Check, Booking element requires {requiredField} elements for reservation.");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
