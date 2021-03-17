using System;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks
{
    public class BookingElementCheck: IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            var bookingElements = schema.Pages.SelectMany(page => page.ValidatableElements)
                .Where(element => element.Type.Equals(EElementType.Booking))
                .ToList();

            if (bookingElements.Any())
            {
                bookingElements.ForEach((booking) =>
                {
                    if (string.IsNullOrEmpty(booking.Properties.BookingProvider))
                        integrityCheckResult.AddFailureMessage($"Booking Element Check, Booking element '{booking.Properties.QuestionId}' requires a valid booking provider property on form {schema.FormName}.");

                    if (booking.Properties.AppointmentType == Guid.Empty)
                        integrityCheckResult.AddFailureMessage($"Booking Element Check, Booking element '{booking.Properties.QuestionId}' requires a AppointmentType property on form {schema.FormName}.");

                    if (booking.Properties.OptionalResources.Any())
                    {
                        booking.Properties.OptionalResources.ForEach(resource =>
                        {
                            if (resource.Quantity <= 0)
                                integrityCheckResult.AddFailureMessage($"Booking Element Check, Booking element '{booking.Properties.QuestionId}', optional resources are invalid, cannot have a quantity less than 0 on form {schema.FormName}.");
                                
                            if (resource.ResourceId.Equals(Guid.Empty))
                                integrityCheckResult.AddFailureMessage($"Booking Element Check, Booking element '{booking.Properties.QuestionId}', optional resources are invalid, ResourceId cannot be an empty Guid on form {schema.FormName}.");
                        });
                    }
                });

                if (!schema.Pages.Any(page => page.PageSlug.ToLower()
                            .Equals(BookingConstants.NO_APPOINTMENT_AVAILABLE)))
                    integrityCheckResult.AddFailureMessage($"Booking Element Check, Form contains booking element, but is missing required page with slug {BookingConstants.NO_APPOINTMENT_AVAILABLE} on form {schema.FormName}.");                    

                var additionalRequiredElements = schema.Pages.SelectMany(page => page.ValidatableElements)
                    .Where(element => element.Properties != null 
                            && element.Properties.TargetMapping != null)
                    .Where(element => element.Properties.TargetMapping.ToLower().Equals("customer.firstname")
                            || element.Properties.TargetMapping.ToLower().Equals("customer.lastname"))
                    .ToList();

                if (additionalRequiredElements.Count != 2)
                {
                    integrityCheckResult.AddFailureMessage($"Booking Element Check, Booking element requires customer firstname/lastname elements for reservation on form {schema.FormName}.");
                }
            }
            
            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}