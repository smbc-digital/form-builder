using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.EnabledFor;
using form_builder.Providers.Transforms.ProviderAvailabilityConfiguration;

namespace form_builder.Services.FormAvailabilityService
{
    public interface IFormAvailabilityService
    {
        bool IsAvailable(List<EnvironmentAvailability> availability, string environment);

        Task<bool> AreAllProvidersAvailable(FormSchema schema, string environment);
    }
    public class FormAvailabilityService : IFormAvailabilityService
    {
        private readonly IEnumerable<IEnabledForProvider> _enabledFor;
        private readonly IProviderAvailabilityConfigurationTransformDataProvider _providerAvailabilityConfigProvider;

        public FormAvailabilityService(IEnumerable<IEnabledForProvider> enabledFor, IProviderAvailabilityConfigurationTransformDataProvider providerAvailabilityConfigProvider)
        {
            _enabledFor = enabledFor;
            _providerAvailabilityConfigProvider = providerAvailabilityConfigProvider;
        } 

        public bool IsAvailable(List<EnvironmentAvailability> availability, string environment)
        {
            var environmentAvailability = availability.SingleOrDefault(_ => _.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase));

            if(environmentAvailability is not null && environmentAvailability.EnabledFor is not null && environmentAvailability.EnabledFor.Any())
                return environmentAvailability.EnabledFor.All(_ => _enabledFor.Get(_.Type).IsAvailable(_));

            return environmentAvailability is null || environmentAvailability.IsAvailable;
        }

        public async Task<bool> AreAllProvidersAvailable(FormSchema schema, string environment)
        {
            var providerAvailabilityConfig = await _providerAvailabilityConfigProvider.Get<List<ProviderAvailabilityConfiguration>>();
            var configsForThisEnv = providerAvailabilityConfig.Where(_ => _.Environments.Contains(environment)).ToList();

            foreach (var config in configsForThisEnv)
            {
                var pagesWithMatchingElements = schema.Pages.Where(_ => _.Elements.Any(_ => config.ElementTypes.Contains(_.Type.ToString()) && config.ProviderNames.Contains(_.Properties.Provider))).ToList();

                foreach (var page in pagesWithMatchingElements)
                {
                    foreach (var element in page.Elements)
                    {
                        if (config.ElementTypes.Contains(element.Type.ToString()) 
                            && config.DisabledFor is not null 
                            && config.DisabledFor.Any()
                            && config.DisabledFor.All(_ => _enabledFor.Get(_.Type).IsNotAvailable(_)))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
