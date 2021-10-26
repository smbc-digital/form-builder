using System;
using System.Net;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Providers.Analytics;
using form_builder.Providers.Analytics.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Analytics
{
    public class GoogleAnalyticsProviderTests
    {
        private readonly GoogleAnalyticsProvider _analyticsProvider;
        private readonly Mock<ILogger<IAnalyticsProvider>> _mockLogger = new ();
        private readonly Mock<IGateway> _mockGateway = new ();
        private readonly Mock<IOptions<GoogleAnalyticsConfiguration>> _mockGoogleAnalyticsOptions = new ();
        private readonly GoogleAnalyticsConfiguration _mockGAConfiguration = new () {  };

        public GoogleAnalyticsProviderTests()
        {
            _mockGoogleAnalyticsOptions.Setup(_ => _.Value).Returns(_mockGAConfiguration);
            _analyticsProvider = new GoogleAnalyticsProvider(_mockLogger.Object, _mockGateway.Object, _mockGoogleAnalyticsOptions.Object);
        }

        [Fact]
        public async Task RaiseEventAsync_Should_Call_Gateway_ToRaise_Event()
        {
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new System.Net.Http.HttpResponseMessage{ StatusCode = HttpStatusCode.OK });

            await _analyticsProvider.RaiseEventAsync(new AnalyticsEventRequest{ EventType = EAnalyticsEventType.Finish, Form = "test-form" });

            _mockGateway.Verify(_ => _.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RaiseEventAsync_Should_Log_Error_If_Response_IsNot_Successfully_StatusCode()
        {
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new System.Net.Http.HttpResponseMessage{ StatusCode = HttpStatusCode.InternalServerError });

            await _analyticsProvider.RaiseEventAsync(new AnalyticsEventRequest{ EventType = EAnalyticsEventType.Finish, Form = "test-form" });

            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task RaiseEventAsync_Should_Log_Error_If_gateway_ThrowsException()
        {
            string formName = "test-form";
            EAnalyticsEventType eventType = EAnalyticsEventType.Finish;
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("an error"));

            await _analyticsProvider.RaiseEventAsync(new AnalyticsEventRequest{ EventType = eventType, Form = formName });

            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}
