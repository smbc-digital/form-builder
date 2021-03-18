using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks;
using form_builder.Validators.IntegrityChecks.FormSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Validators.FormSchema.IntegrityChecks.FormSchema
{
    public class BookingFormCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(Models.FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            if (schema.Pages.Any(page => page.Elements.Any(element => element.Type.Equals(EElementType.Booking))))
                return integrityCheckResult;

            if (!schema.Pages.Any(page => page.PageSlug.Equals(BookingConstants.NO_APPOINTMENT_AVAILABLE, StringComparison.OrdinalIgnoreCase)))
                integrityCheckResult.AddFailureMessage($"Booking Element Check, Form contains booking element, but is missing required page with slug {BookingConstants.NO_APPOINTMENT_AVAILABLE}.");

            var additionalRequiredElements = schema.Pages
                .SelectMany(page => page.ValidatableElements)
                .Where(element => element.Properties is not null
                        && element.Properties.TargetMapping is not null)
                .Where(element => element.Properties.TargetMapping.ToLower().Equals("customer.firstname")
                        || element.Properties.TargetMapping.ToLower().Equals("customer.lastname"))
                .ToList();

            if (additionalRequiredElements.Count != 2)
            {
                integrityCheckResult.AddFailureMessage($"Booking Element Check, Booking element requires customer firstname/lastname elements for reservation.");
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(Models.FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
