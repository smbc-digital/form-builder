using AnalyticsConfiguration = form_builder.Configuration.AnalyticsConfiguration;

namespace form_builder.Services.AnalyticsService;

public class AnalyticsService(IEnumerable<IAnalyticsProvider> analyticsProviders,
    IOptions<AnalyticsConfiguration> configuration)
    : IAnalyticsService
{
    public void RaiseEvent(string form, EAnalyticsEventType eventType)
    {
        if (configuration.Value.Enabled)
        {
            var provider = analyticsProviders.Get(configuration.Value.Type);
            provider.RaiseEventAsync(new AnalyticsEventRequest { EventType = eventType, Form = form });
        }
    }
}