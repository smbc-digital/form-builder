using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Providers.Analytics.Entities;
using form_builder.Providers.Analytics.Request;
using Microsoft.Extensions.Options;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Providers.Analytics
{
    public class GoogleAnalyticsProvider : IAnalyticsProvider
    {
        public string ProviderName { get => "GA"; }
        private ILogger<IAnalyticsProvider> _logger;
        private IGateway _gateway;
        private IOptions<GoogleAnalyticsConfiguration> _configuration;

        public GoogleAnalyticsProvider(ILogger<IAnalyticsProvider> logger, IGateway gateway, IOptions<GoogleAnalyticsConfiguration> configuration)
        {
            _logger = logger;
            _gateway = gateway;
            _configuration = configuration;
        }

        public async Task RaiseEventAsync(AnalyticsEventRequest request)
        {
            try
            {
                var googleanalyticsEventOptions = ToModel(request);
                var payload = new GoogleAnalyticsEventPayload
                {
                    TrackingId = _configuration.Value.TrackingId,
                    EventCategory = googleanalyticsEventOptions.EventCategory,
                    EventAction = googleanalyticsEventOptions.EventAction,
                    EventLabel = googleanalyticsEventOptions.EventLabel,
                    ClientId = _configuration.Value.ClientId
                };

                var result = await _gateway.GetAsync(payload.ToString(_configuration.Value.ApiUrl));

                if (!result.IsSuccessStatusCode)
                    throw new ApplicationException($"GoogleAnalyticsProvider::RaiseEventAsync gateway returned a unsuccessful status code, {result.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"GoogleAnalyticsProvider::RaiseEventAsync, failed to send event to {ProviderName} for {request.EventType} on form {request.Form}. Exception: {ex.Message}");
            }
        }

        private GoogleAnalyticsEntity ToModel(AnalyticsEventRequest request)
        {
            var eventAnalyticsConfig = _configuration.Value.Events.FirstOrDefault(_ => _.EventType.Equals(request.EventType.ToString()));

            if (eventAnalyticsConfig is null)
                throw new ApplicationException($"GoogleAnalyticsProvider::ToModel, Failed to find analytics configuration for event {request.EventType} within google analytics configuration");

            return request.EventType switch
            {
                EAnalyticsEventType.Finish => new GoogleAnalyticsEntity
                {
                    EventCategory = eventAnalyticsConfig.AnalyticsCategory,
                    EventAction = eventAnalyticsConfig.AnalyticsAction,
                    EventLabel = request.Form,
                },
                _ => throw new ArgumentOutOfRangeException($"GoogleAnalyticsProvider::ToModel, Failed to convert analytics event {request.EventType} to google analytics entity")
            };
        }
    }
}