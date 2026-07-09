using form_builder.Providers.Analytics.Request;
using AnalyticsConfiguration = form_builder.Configuration.AnalyticsConfiguration;

namespace form_builder.Services.AnalyticsService;

public class AnalyticsService(
    IEnumerable<IAnalyticsProvider> analyticsProviders,
    IOptions<AnalyticsConfiguration> configuration)
    : IAnalyticsService
{
    private readonly IEnumerable<IAnalyticsProvider> _analyticsProviders = analyticsProviders;
    private readonly IOptions<AnalyticsConfiguration> _configuration = configuration;

    public void RaiseEvent(string form, EAnalyticsEventType eventType)
    {
        if (_configuration.Value.Enabled)
        {
            var provider = _analyticsProviders.Get(_configuration.Value.Type);
            provider.RaiseEventAsync(new AnalyticsEventRequest { EventType = eventType, Form = form });
        }
    }
}