using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
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
                result.AddFailureMessage(BookingConstants.INTEGRITY_FAILURE_MESSAGE_NOAPPOINTMENTPAGE);

            var pagesWithElements = schema.Pages
                .Where(page => page.Elements is not null);

            var bookingElements = pagesWithElements
                .SelectMany(page => page.Elements
                    .Where(element => element.Type
                        .Equals(EElementType.Booking)));

            if (bookingElements.Select(element => element.Properties.BookingProvider).Distinct().Count() > 1)
                result.AddFailureMessage(BookingConstants.INTEGRITY_FAILURE_MESSAGE_DUPLICATEPROVIDER);

            var validatableElements = pagesWithElements.SelectMany(page => page.ValidatableElements);

            foreach (var bookingElement in bookingElements)
            {
                foreach (var appointmentType in bookingElement.Properties.AppointmentTypes)
                {
                    if (!appointmentType.NeedsAppointmentIdMapping)
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
                            result.AddFailureMessage(BookingConstants.INTEGRITY_FAILURE_MESSAGE_APPOINTMENTIDKEY_SOURCE_NOTONPREVIOUSPAGE);

                        foreach (var option in sourceElement.Properties.Options)
                        {
                            if (!Guid.TryParse(option.Value, out Guid _))
                            {
                                result.AddFailureMessage($"{BookingConstants.INTEGRITY_FAILURE_MESSAGE_APPOINTMENTIDKEY_SOURCE_VALUENOTGUID} Check Option wth \"Text\": \"{option.Text}\" on {appointmentType.Environment}.");
                            }
                        }
                    }
                    else
                    {
                        result.AddFailureMessage(BookingConstants.INTEGRITY_FAILURE_MESSAGE_APPOINTMENTIDKEY_DOESNOTEXIST);
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
                    result.AddFailureMessage($"{BookingConstants.INTEGRITY_FAILURE_MESSAGE_REQUIREDFIELDS} {requiredField} required for booking api.");
                }
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
