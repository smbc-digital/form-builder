using System.Collections.Generic;

namespace form_builder.Configuration
{
    public class GoogleAnalyticsConfiguration
    {
        public const string ConfigValue = "GoogleAnalyticsConfiguration";
        public string ApiUrl { get; set; }
        public string TrackingId { get; set; }
        public string ClientId { get; set; }
        public List<GoogleAnalyticsEvent> Events { get; set; }
    }
}