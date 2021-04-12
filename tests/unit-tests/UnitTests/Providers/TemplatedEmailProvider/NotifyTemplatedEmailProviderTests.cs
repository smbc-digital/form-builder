using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using form_builder.Providers.TemplatedEmailProvider;
using Microsoft.Extensions.Logging;
using Moq;
using Notify.Interfaces;
using Notify.Models.Responses;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.TemplatedEmailProvider
{
    public class NotifyTemplatedEmailProviderTests
    {
        private readonly NotifyTemplatedEmailProvider _notifyProvider;
        private readonly Mock<IAsyncNotificationClient> _mockClient = new Mock<IAsyncNotificationClient>();
        private readonly Mock<ILogger<NotifyTemplatedEmailProvider>> _mockLogger = new Mock<ILogger<NotifyTemplatedEmailProvider>>();

        public NotifyTemplatedEmailProviderTests()
        {
            _notifyProvider = new NotifyTemplatedEmailProvider(_mockClient.Object, _mockLogger.Object);
        }

        [Fact]
        public void SendEmailAsync_ShouldCallLogger_IfExceptionThrown()
        {
            // Arrange
            _mockClient
                .Setup(_ => _.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("error"));

            // Act
            _notifyProvider.SendEmailAsync("emailAddress", "templateId", new Dictionary<string, dynamic>());

            // Assert
            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}
