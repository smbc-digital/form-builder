using form_builder.Providers.Analytics.Request;

namespace form_builder.Providers.Analytics;

public class FakeAnalyticsProvider(ILogger<IAnalyticsProvider> logger) : IAnalyticsProvider
{
    public string ProviderName { get => "Fake"; }
    private ILogger<IAnalyticsProvider> _logger = logger;

    public async Task RaiseEventAsync(AnalyticsEventRequest request) =>
        _logger.LogInformation($"FakeAnalyticsProvider raised event for {request.EventType} on form {request.Form}");
}