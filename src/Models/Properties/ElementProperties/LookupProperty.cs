namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public List<LookupSource> LookupSources { get; set; }
    }
    public class LookupSource
    {
        public string EnvironmentName { get; set; }

        public string Provider { get; set; }

        public string URL { get; set; }

        public string AuthToken { get; set; }
    }
}
