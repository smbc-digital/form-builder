using System.Threading.Tasks;
using form_builder.Providers.Analytics.Request;
using Microsoft.Extensions.Logging;

namespace form_builder.Providers.Analytics
{
    public class FakeAnalyticsProvider : IAnalyticsProvider
    {
        public string ProviderName { get => "Fake"; }
        private ILogger<IAnalyticsProvider> _logger;

        public FakeAnalyticsProvider(ILogger<IAnalyticsProvider> logger)
            => _logger = logger;

        public async Task RaiseEventAsync(AnalyticsEventRequest request) =>
            _logger.LogInformation($"FakeAnalyticsProvider raised event for {request.EventType} on form {request.Form}");
    }
}