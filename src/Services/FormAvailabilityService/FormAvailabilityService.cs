using form_builder.Extensions;
using form_builder.Models;
using form_builder.Providers.EnabledFor;
using form_builder.Restrictions;

namespace form_builder.Services.FormAvailabilityService
{
    public interface IFormAvailabilityService
    {
        bool IsAvailable(List<EnvironmentAvailability> availability, string environment);
        bool IsFormAccessApproved(FormSchema baseForm);
    }

    public class FormAvailabilityService : IFormAvailabilityService
    {
        private readonly IEnumerable<IEnabledForProvider> _enabledFor;

        private readonly IEnumerable<IFormAccessRestriction> _formAccessRestrictions;

        public FormAvailabilityService(IEnumerable<IEnabledForProvider> enabledFor, IEnumerable<IFormAccessRestriction> formAccessRestrictions)
        {
            _enabledFor = enabledFor;
            _formAccessRestrictions = formAccessRestrictions;
        }

        public bool IsAvailable(List<EnvironmentAvailability> availability, string environment)
        {
            var environmentAvailability = availability.SingleOrDefault(_ => _.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase));

            if (environmentAvailability is not null && environmentAvailability.EnabledFor is not null && environmentAvailability.EnabledFor.Any())
                return environmentAvailability.EnabledFor.All(_ => _enabledFor.Get(_.Type).IsAvailable(_));

            return environmentAvailability is null || environmentAvailability.IsAvailable;
        }

        public bool IsFormAccessApproved(FormSchema baseForm) => !_formAccessRestrictions.Any(restriction => restriction.IsRestricted(baseForm));
    }
}
