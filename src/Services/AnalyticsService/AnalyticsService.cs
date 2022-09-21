using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Providers.Analytics;
using form_builder.Providers.Analytics.Request;
using Microsoft.Extensions.Options;

namespace form_builder.Services.AnalyticsService
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IEnumerable<IAnalyticsProvider> _analyticsProviders;
        private readonly IOptions<AnalyticsConfiguration> _configuration;
        public AnalyticsService(IEnumerable<IAnalyticsProvider> analyticsProviders, IOptions<AnalyticsConfiguration> configuration)
        {
            _analyticsProviders = analyticsProviders;
            _configuration = configuration;
        }

        public void RaiseEvent(string form, EAnalyticsEventType eventType)
        {
            if (_configuration.Value.Enabled)
            {
                var provider = _analyticsProviders.Get(_configuration.Value.Type);
                provider.RaiseEventAsync(new AnalyticsEventRequest { EventType = eventType, Form = form });
            }
        }
    }
}