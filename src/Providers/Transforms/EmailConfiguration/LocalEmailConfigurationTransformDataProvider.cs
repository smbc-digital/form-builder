using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.EmailConfiguration
{
    public class LocalEmailConfigurationTransformDataProvider : IEmailConfigurationTransformDataProvider
    {
        public async Task<T> Get<T>()
        {
            return JsonConvert.DeserializeObject<T>(
                await File.ReadAllTextAsync($@".\DSL\email-config\emailconfiguration.json"));
        }
    }
}
