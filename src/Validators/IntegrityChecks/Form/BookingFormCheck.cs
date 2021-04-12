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
                result.AddFailureMessage($"Booking Form Check: Contains booking element, but is missing required page with slug {BookingConstants.NO_APPOINTMENT_AVAILABLE}.");

            var pagesWithElements = schema.Pages
                .Where(page => page.Elements is not null);

            var bookingElements = pagesWithElements
                .SelectMany(page => page.Elements
                    .Where(element => element.Type
                        .Equals(EElementType.Booking)));

            if (bookingElements.Select(element => element.Properties.BookingProvider).Distinct().Count() > 1)
                result.AddFailureMessage($"Booking Form Check: Contains different booking provider. Only one provider allows on for form");

            var validatableElements = pagesWithElements.SelectMany(page => page.ValidatableElements);

            foreach (var bookingElement in bookingElements)
            {
                foreach (var appointmentType in bookingElement.Properties.AppointmentTypes)
                {
                    if (appointmentType.AppointmentId.Equals(Guid.Empty) && !bookingElement.Properties.BookingProvider.Equals(BookingConstants.FAKE_PROVIDER, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(appointmentType.AppointmentIdKey))
                        {
                            result.AddFailureMessage($"Booking Form Check: Contains empty Guid as AppointmentId and no AppointmentIdKey on {appointmentType.Environment}, with a non-fake provider");
                        }
                        else
                        {
                            if (!validatableElements.Any(element => element.Properties.QuestionId is not null && element.Properties.QuestionId.Equals(appointmentType.AppointmentIdKey, StringComparison.Ordinal)))
                                result.AddFailureMessage($"Booking Form Check: AppointmentIdKey does not exist... check corresponding QuestionId on your previous page.");
                        }
                    }
                }
            }

            var requiredFields = new[] { "customer.firstname", "customer.lastname" };
            foreach (var requiredField in requiredFields)
            {
                if (!schema.Pages.Any(page => page.Elements.Any(element =>
                    element.Properties is not null &&
                    element.Properties.TargetMapping is not null &&
                    element.Properties.TargetMapping.Equals(requiredField, StringComparison.OrdinalIgnoreCase))))
                {
                    result.AddFailureMessage($"Booking Element Check, Booking element requires {requiredField} elements for reservation.");
                }
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
