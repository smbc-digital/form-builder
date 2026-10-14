namespace form_builder.Providers.Analytics;

public interface IAnalyticsProvider
{
    string ProviderName { get; }
    Task RaiseEventAsync(AnalyticsEventRequest request);
}