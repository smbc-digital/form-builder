using System.Collections.Generic;
using System.Linq;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Providers.EnabledFor;

namespace form_builder.Services.FormAvailabilityService
{
    public interface IFormAvailabilityService
    {
        bool IsAvailable(List<EnvironmentAvailability> availability, string environment);
    }
    public class FormAvailabilityService : IFormAvailabilityService
    {
        private readonly IEnumerable<IEnabledForProvider> _enabledFor;
        public FormAvailabilityService(IEnumerable<IEnabledForProvider> enabledFor) => _enabledFor = enabledFor;

        public bool IsAvailable(List<EnvironmentAvailability> availability, string environment)
        {
            var environmentAvailability = availability.SingleOrDefault(_ => _.Environment.ToLower().Equals(environment.ToLower()));

            if(environmentAvailability is not null && environmentAvailability.EnabledFor is not null && environmentAvailability.EnabledFor.Any())
                return environmentAvailability.EnabledFor.All(_ => _enabledFor.Get(_.Type).IsAvailable(_));

            return environmentAvailability == null || environmentAvailability.IsAvailable;
        }
    }
}
