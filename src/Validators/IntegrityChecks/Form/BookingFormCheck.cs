using System;
using System.Linq;
using System.Threading.Tasks;
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
                    if (!appointmentType.NeedsMapping)
                        continue;

                    IElement sourceElement = validatableElements
                        .SingleOrDefault(element =>
                            element.Properties.QuestionId is not null &&
                            element.Properties.QuestionId
                                .Equals(appointmentType.AppointmentIdKey, StringComparison.Ordinal));

                    if (sourceElement is not null && sourceElement.Properties.Options is not null)
                    {
                        int pageNumber = 0, sourceElementPage = 0, bookingElementPage = 0;
                        foreach (var page in pagesWithElements)
                        {
                            if (page.ValidatableElements.Any(element => element.Properties.QuestionId is not null && element.Properties.QuestionId.Equals(appointmentType.AppointmentIdKey, StringComparison.Ordinal)))
                                sourceElementPage = pageNumber;

                            if (page.Elements.Any(element => element.Equals(bookingElement)))
                                bookingElementPage = pageNumber;

                            pageNumber++;
                        }

                        if (sourceElementPage >= bookingElementPage)
                            result.AddFailureMessage($"Booking Form Check: Source For AppointmentIdKey is not on previous page.");

                        foreach (var option in sourceElement.Properties.Options)
                        {
                            if (!Guid.TryParse(option.Value, out Guid _))
                            {
                                result.AddFailureMessage($"Booking Form Check: AppointmentIdKey Value on {appointmentType.Environment} is not a Guid. Check value for option= \"Text\": \"{option.Text}\" .");
                            }
                        }
                    }
                    else
                    {
                        result.AddFailureMessage($"Booking Form Check: AppointmentIdKey does not exist... check corresponding QuestionId on your previous page.");
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
