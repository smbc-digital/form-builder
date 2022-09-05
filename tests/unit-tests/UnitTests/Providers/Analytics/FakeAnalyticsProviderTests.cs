using form_builder.Enum;
using form_builder.Providers.Analytics;
using form_builder.Providers.Analytics.Request;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Analytics
{
    public class FakeAnalyticsProviderTests
    {
        private readonly FakeAnalyticsProvider _analyticsProvider;
        private readonly Mock<ILogger<IAnalyticsProvider>> _mockLogger = new();

        public FakeAnalyticsProviderTests()
            => _analyticsProvider = new FakeAnalyticsProvider(_mockLogger.Object);

        [Fact]
        public async Task RaiseEventAsync_Should_Log_AnalyticsEvent()
        {
            string formName = "test-form";
            EAnalyticsEventType eventType = EAnalyticsEventType.Finish;

            await _analyticsProvider.RaiseEventAsync(new AnalyticsEventRequest { EventType = eventType, Form = formName });

            _mockLogger.Verify(_ => _.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}
