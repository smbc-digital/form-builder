namespace form_builder.Configuration
{
    public class NotifyConfiguration
    {
        public const string ConfigValue = "NotifyConfiguration";
        public string Key { get; set; }
        public EmailTokens EmailTokens { get; set; }
        public string BaseUrl { get; set; }
    }

    public class EmailTokens
    {
        public string StreetLighting { get; set; }
    }
}
