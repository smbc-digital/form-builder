using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.EmailConfiguration
{
    public class EmailConfigurationTransformDataProvider : IEmailConfigurationTransformDataProvider
    {
        public async Task<T> Get<T>()
        {
            return JsonConvert.DeserializeObject<T>(
                await System.IO.File.ReadAllTextAsync($@".\DSL\email-config\emailconfiguration.json"));
        }
    }
}
