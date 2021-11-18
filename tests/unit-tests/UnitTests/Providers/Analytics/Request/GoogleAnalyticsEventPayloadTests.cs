using form_builder.Providers.Analytics.Request;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Analytics
{
    public class GoogleAnalyticsEventPayloadTests
    {

        [Fact]
        public void ToString_ShouldBuild_Valid_Url()
        {
            var baseUrl = "https://www.stockport.gov.uk";
            var clientId = "CLIENT_ID";
            var trackingId = "TRACKING_ID";
            var eventCategory = "EVENT_CAT";
            var eventAction = "EVENT_ACTION";
            var eventLabel = "EVENT_LABEL";
            var eventPayload = new GoogleAnalyticsEventPayload { ClientId = clientId, TrackingId = trackingId, EventCategory = eventCategory, EventAction = eventAction, EventLabel = eventLabel };
            var url = $"{baseUrl}?v=1&cid={clientId}&t=event&tid={trackingId}&ec={eventCategory}&ea={eventAction}&el={eventLabel}";
            
            var result = eventPayload.ToString(baseUrl);

            Assert.Equal(url, result);
        }
    }
}
