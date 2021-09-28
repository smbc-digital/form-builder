using System;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Providers.Analytics.Request;
using Microsoft.Extensions.Logging;
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
                var payload = new GoogleAnalyticsEventPayload
                {
                    TrackingId = _configuration.Value.TrackingId,
                    EventCategory = "useraction",
                    EventAction = request.EventType.ToString(),
                    EventLabel = request.Form,
                    ClientId = _configuration.Value.ClientId
                };

                var result = await _gateway.GetAsync(payload.ToString(_configuration.Value.ApiUrl));

                if(!result.IsSuccessStatusCode)
                    throw new ApplicationException("GoogleAnalyticsProvider::RaiseEventAsync gateway retuened unsuccessful status code");
            }
            catch (Exception ex)
            {
                _logger.LogError($"GoogleAnalyticsProvider::RaiseEventAsync, failed to send event to {ProviderName} for {request.EventType} on form {request.Form}. Exception: {ex.Message}");
            }
        }
    }
}