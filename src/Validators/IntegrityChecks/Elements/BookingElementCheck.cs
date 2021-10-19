using System;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;
using Microsoft.AspNetCore.Hosting;

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

            AppointmentType appointmentTypeForEnv = element.Properties.AppointmentTypes
                .FirstOrDefault(appointmentType => appointmentType.Environment.Equals(_environment.EnvironmentName.ToS3EnvPrefix(), StringComparison.OrdinalIgnoreCase));

            if (appointmentTypeForEnv is null)
            {
                result.AddFailureMessage("Booking Element Check, No AppointmentType found for current environment.");
                return result;
            }

            if (appointmentTypeForEnv.AppointmentId.Equals(Guid.Empty) &&
                string.IsNullOrEmpty(appointmentTypeForEnv.AppointmentIdKey))
            {
                result.AddFailureMessage("Booking Element Check, You must supply either an AppointmentId or an AppointmentIdKey in the AppointmentType.");
            }

            if (!appointmentTypeForEnv.AppointmentId.Equals(Guid.Empty) &&
                !string.IsNullOrEmpty(appointmentTypeForEnv.AppointmentIdKey))
            {
                result.AddFailureMessage("Booking Element Check, You cannot use both AppointmentId and AppointmentIdKey in the AppointmentType.");
            }

            if (appointmentTypeForEnv.OptionalResources.Count.Equals(0))
                return result;

            foreach (var resource in appointmentTypeForEnv.OptionalResources)
            {
                if (resource.Quantity <= 0)
                    result.AddFailureMessage($"Booking Element Check, Booking element '{element.Properties.QuestionId}', optional resources are invalid, cannot have a quantity of 0 or less.");

                if (resource.ResourceId.Equals(Guid.Empty))
                    result.AddFailureMessage($"Booking Element Check, Booking element '{element.Properties.QuestionId}', optional resources are invalid, ResourceId cannot be an empty Guid.");
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}
