namespace form_builder.Providers.Analytics.Request
{
    public class AnalyticsEventRequest
    {
        public string Form { get; set; }
        public EAnalyticsEventType EventType { get; set; }
    }

    public enum EAnalyticsEventType 
    {
        Unknown,
        Start,
        Finish,
        Error  
    }
}
