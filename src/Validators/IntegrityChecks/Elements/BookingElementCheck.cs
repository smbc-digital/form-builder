using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class BookingElementCheck : IElementSchemaIntegrityCheck
    {
        private readonly IWebHostEnvironment _environment;
        public BookingElementCheck(IWebHostEnvironment enviroment) =>
            _environment = enviroment;

        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (!element.Type.Equals(EElementType.Booking))
                return result;

            if (string.IsNullOrEmpty(element.Properties.BookingProvider))
                result.AddFailureMessage($"Booking Element Check, Booking element '{element.Properties.QuestionId}' requires a valid booking provider property.");

            var appointmentTypeForEnv = element.Properties.AppointmentTypes
                .FirstOrDefault(appointmentType => appointmentType.Environment.Equals(_environment.EnvironmentName, StringComparison.OrdinalIgnoreCase));

            if (appointmentTypeForEnv is null)
            {
                result.AddFailureMessage($"Booking Element Check, No appointment type found for current environment or empty AppointmentID.");
            }
            else if (appointmentTypeForEnv.OptionalResources.Any())
            {
                appointmentTypeForEnv.OptionalResources.ForEach(resource =>
                {
                    if (resource.Quantity <= 0)
                        result.AddFailureMessage($"Booking Element Check, Booking element '{element.Properties.QuestionId}', optional resources are invalid, cannot have a quantity less than 0.");

                    if (resource.ResourceId.Equals(Guid.Empty))
                        result.AddFailureMessage($"Booking Element Check, Booking element '{element.Properties.QuestionId}', optional resources are invalid, ResourceId cannot be an empty Guid.");
                });
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}
