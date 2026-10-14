namespace form_builder.Providers.Analytics;

public class FakeAnalyticsProvider(ILogger<IAnalyticsProvider> logger) : IAnalyticsProvider
{
    public string ProviderName => "Fake";

    public async Task RaiseEventAsync(AnalyticsEventRequest request) =>
        logger.LogInformation($"FakeAnalyticsProvider raised event for {request.EventType} on form {request.Form}");
}