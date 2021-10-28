using System;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Extensions;
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
                    EventCategory = AnalyticsConstants.EVENT_CATEGORY,
                    EventAction = request.EventType.ToReadableTextForAnlayticsEvent(),
                    EventLabel = request.Form,
                    ClientId = _configuration.Value.ClientId
                };

                var result = await _gateway.GetAsync(payload.ToString(_configuration.Value.ApiUrl));

                if(!result.IsSuccessStatusCode)
                    throw new ApplicationException($"GoogleAnalyticsProvider::RaiseEventAsync gateway returned a unsuccessful status code, {result.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"GoogleAnalyticsProvider::RaiseEventAsync, failed to send event to {ProviderName} for {request.EventType} on form {request.Form}. Exception: {ex.Message}");
            }
        }
    }
}