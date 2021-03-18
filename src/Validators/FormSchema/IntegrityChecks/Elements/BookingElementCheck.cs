using System;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models.Elements;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class BookingElementCheck : IElementSchemaIntegrityCheck
    {
        private readonly IWebHostEnvironment _environment;
        public BookingElementCheck(IWebHostEnvironment enviroment) => _environment = enviroment;

        public IntegrityCheckResult Validate(IElement element)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            if (!element.Type.Equals(EElementType.Booking))
                return integrityCheckResult;

            if (string.IsNullOrEmpty(element.Properties.BookingProvider))
                integrityCheckResult.AddFailureMessage($"Booking Element Check, Booking element '{element.Properties.QuestionId}' requires a valid booking provider property.");

            var appointmentTypeForEnv = element.Properties.AppointmentTypes
                .FirstOrDefault(_ => _.Environment.Equals(_environment.EnvironmentName, StringComparison.OrdinalIgnoreCase));

            if (appointmentTypeForEnv is null)
            {
                integrityCheckResult.AddFailureMessage($"Booking Element Check, No appointment type found for current environment or empty AppointmentID.");
            }
            else if (appointmentTypeForEnv.OptionalResources.Any())
            {
                appointmentTypeForEnv.OptionalResources.ForEach(resource =>
                {
                    if (resource.Quantity <= 0)
                        integrityCheckResult.AddFailureMessage($"Booking Element Check, Booking element '{element.Properties.QuestionId}', optional resources are invalid, cannot have a quantity less than 0.");

                    if (resource.ResourceId.Equals(Guid.Empty))
                        integrityCheckResult.AddFailureMessage($"Booking Element Check, Booking element '{element.Properties.QuestionId}', optional resources are invalid, ResourceId cannot be an empty Guid.");
                });
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}
