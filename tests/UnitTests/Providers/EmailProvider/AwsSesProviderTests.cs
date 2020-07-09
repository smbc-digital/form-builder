using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using form_builder.Models;
using form_builder.Providers.EmailProvider;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.EmailProvider
{
    public class AwsSesProviderTests
    {
        private readonly IEmailProvider _provider;
        private readonly Mock<IAmazonSimpleEmailService> _mockEmailService = new Mock<IAmazonSimpleEmailService>();
        private readonly Mock<ILogger<AwsSesProvider>> _mockLogger = new Mock<ILogger<AwsSesProvider>>();

        public AwsSesProviderTests()
        {
            _mockEmailService.Setup(_ => _.SendRawEmailAsync(It.IsAny<SendRawEmailRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendRawEmailResponse { HttpStatusCode = HttpStatusCode.OK, MessageId = "test", ResponseMetadata = new ResponseMetadata { RequestId = "test" } });

            _provider = new AwsSesProvider(_mockEmailService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SendAwsSesEmail_ShouldReturnInternalServerError_If_ToEmailIsNull()
        {
            // Arrange
            var emailMessage = new EmailMessage("subject", "body", "from", "");

            // Act
            var result = await _provider.SendAwsSesEmail(emailMessage);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result);
        }

        [Fact]
        public async Task SendAwsSesEmail_ShouldReturnOk_If_ToEmailIsNotNull()
        {
            // Arrange
            var emailMessage = new EmailMessage("subject", "body", "from@email.com", "to@email.com");
            
            // Act
            var result = await _provider.SendAwsSesEmail(emailMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
        }

        [Fact]
        public async Task SendAwsSesEmail_ShouldReturnBadRequest_If_EmailServiceThrows()
        {
            // Arrange
            var emailMessage = new EmailMessage("subject", "body", "from@email.com", "to@email.com");
            _mockEmailService.Setup(_ => _.SendRawEmailAsync(It.IsAny<SendRawEmailRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _provider.SendAwsSesEmail(emailMessage);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result);
        }
    }
}