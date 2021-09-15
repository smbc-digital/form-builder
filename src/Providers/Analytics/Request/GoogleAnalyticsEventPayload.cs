namespace form_builder.Providers.Analytics.Request
{
    public class GoogleAnalyticsEventPayload
    {
        public string TrackingId { get; set; }
        public string EventCategory { get; set; }
        public string EventAction { get; set; }
        public string EventLabel { get; set; }
        public string ClientId { get; set; }

        public string ToString(string baseUrl) => string.Format(
                "{0}?v=1&cid={1}&t=event&tid={2}&ec={3}&ea={4}&el={5}",
                baseUrl,
                ClientId,
                TrackingId,
                EventCategory,
                EventAction,
                EventLabel);
    }
}