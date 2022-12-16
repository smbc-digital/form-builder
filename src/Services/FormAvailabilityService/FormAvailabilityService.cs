using form_builder.Extensions;
using form_builder.Models;
using form_builder.Providers.EnabledFor;

namespace form_builder.Services.FormAvailabilityService
{
    public interface IFormAvailabilityService
    {
        bool IsAvailable(List<EnvironmentAvailability> availability, string environment);
        bool HaveFormAccessPreRequirsitesBeenMet(FormSchema baseForm);
    }

    public class FormAvailabilityService : IFormAvailabilityService
    {
        private readonly IEnumerable<IEnabledForProvider> _enabledFor;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public FormAvailabilityService(IEnumerable<IEnabledForProvider> enabledFor, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _enabledFor = enabledFor;
        }

        public bool IsAvailable(List<EnvironmentAvailability> availability, string environment)
        {
            var environmentAvailability = availability.SingleOrDefault(_ => _.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase));

            if (environmentAvailability is not null && environmentAvailability.EnabledFor is not null && environmentAvailability.EnabledFor.Any())
                return environmentAvailability.EnabledFor.All(_ => _enabledFor.Get(_.Type).IsAvailable(_));

            return environmentAvailability is null || environmentAvailability.IsAvailable;
        }

        public bool HaveFormAccessPreRequirsitesBeenMet(FormSchema baseForm)
        {
                if(!string.IsNullOrEmpty(baseForm.Key) && !string.IsNullOrEmpty(baseForm.KeyName))
                {
                    var context = _httpContextAccessor.HttpContext;
                    if(!_httpContextAccessor.HttpContext.Request.Query.Any(KeyValuePair => KeyValuePair.Key == baseForm.KeyName))
                        return false;
                    
                    var keyValuePair = _httpContextAccessor.HttpContext.Request.Query.SingleOrDefault(KeyValuePair => KeyValuePair.Key == baseForm.KeyName);
                    if(!keyValuePair.Value.Equals(baseForm.Key))
                        return false;
                }

                return true;
        }
    }
}
