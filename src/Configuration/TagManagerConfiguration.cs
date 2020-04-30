using Microsoft.Extensions.Configuration;

namespace form_builder.Configuration
{
    public class TagManagerConfiguration : ITagManagerConfiguration
    {
        public TagManagerConfiguration(IConfiguration configuration)
        {
            TagManagerId = configuration.GetValue<string>("GoogleTagManagerId");
        }

        public string TagManagerId { get; set; }
    }
}