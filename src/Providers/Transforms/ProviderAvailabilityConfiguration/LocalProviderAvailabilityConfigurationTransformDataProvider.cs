using System.Threading.Tasks;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.ProviderAvailabilityConfiguration
{
    public class LocalProviderAvailabilityConfigurationTransformDataProvider : IProviderAvailabilityConfigurationTransformDataProvider
    {
        public async Task<T> Get<T>()
        {
            return JsonConvert.DeserializeObject<T>(
                await System.IO.File.ReadAllTextAsync($@".\DSL\provider-availability-config\provider-availability-configuration.json"));
        }
    }
}
