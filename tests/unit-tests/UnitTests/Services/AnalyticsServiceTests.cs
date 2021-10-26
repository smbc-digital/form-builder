using System.Collections.Generic;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Providers.Analytics;
using form_builder.Providers.Analytics.Request;
using form_builder.Services.AnalyticsService;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class AnalyticsServiceTests
    {
        private readonly AnalyticsService _service;

        private readonly Mock<IAnalyticsProvider> _mockAnalyticsProvider = new();
        private readonly IEnumerable<IAnalyticsProvider> _mockAnalyticsProviders;
        private readonly Mock<IOptions<AnalyticsConfiguration>> _mockAnalyticsConfiguration = new();
        private readonly AnalyticsConfiguration _analyiticsConfig = new() { Enabled = true, Type = "testAnalyticsProvider" };
        public AnalyticsServiceTests()
        {
            _mockAnalyticsProvider.Setup(_ => _.ProviderName).Returns("testAnalyticsProvider");
            _mockAnalyticsProviders = new List<IAnalyticsProvider>
            {
                _mockAnalyticsProvider.Object
            };

            _mockAnalyticsConfiguration.Setup(_ => _.Value).Returns(_analyiticsConfig);

            _service = new AnalyticsService(_mockAnalyticsProviders, _mockAnalyticsConfiguration.Object);
        }

        [Fact]
        public void RaiseEvent_Should_Call_Configured_Provider()
        {
            // Act
            string form = "test-form";
            EAnalyticsEventType eventType = EAnalyticsEventType.Start;

            _service.RaiseEvent(form, eventType);

            // Assert
            _mockAnalyticsProvider.Verify(_ => _.RaiseEventAsync(It.Is<AnalyticsEventRequest>(_ => _.Form.Equals(form) && _.EventType.Equals(eventType))), Times.Once);
        }

        [Fact]
        public void RaiseEvent_Should_Not_CallProvider_If_Analaytics_IsDisbaled()
        {
            // Act
            _mockAnalyticsConfiguration.Setup(_ => _.Value).Returns(new AnalyticsConfiguration { Enabled = false });

            string form = "test-form";
            EAnalyticsEventType eventType = EAnalyticsEventType.Start;

            _service.RaiseEvent(form, eventType);

            // Assert
            _service.RaiseEvent(form, eventType);
            _mockAnalyticsProvider.Verify(_ => _.RaiseEventAsync(It.Is<AnalyticsEventRequest>(_ => _.Form.Equals(form) && _.EventType.Equals(eventType))), Times.Never);
        }
    }
}
