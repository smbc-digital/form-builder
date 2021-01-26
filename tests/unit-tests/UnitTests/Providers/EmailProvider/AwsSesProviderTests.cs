using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using form_builder.Models;
using form_builder.Providers.EmailProvider;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.EmailProvider
{
    public class AwsSesProviderTests
    {
        private readonly IEmailProvider _provider;
        private readonly Mock<IAmazonSimpleEmailService> _mockEmailService = new Mock<IAmazonSimpleEmailService>();

        public AwsSesProviderTests()
        {
            _mockEmailService.Setup(_ => _.SendRawEmailAsync(It.IsAny<SendRawEmailRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendRawEmailResponse { HttpStatusCode = HttpStatusCode.OK, MessageId = "test", ResponseMetadata = new ResponseMetadata { RequestId = "test" } });

            _provider = new AwsSesProvider(_mockEmailService.Object);
        }

        [Fact]
        public async Task SendAwsSesEmail_Should_ThrowApplicationException_If_ToEmail_IsNull()
        {
            // Arrange
            var emailMessage = new EmailMessage("subject", "body", "from", "");

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _provider.SendEmail(emailMessage));
        }

        [Fact]
        public async Task SendAwsSesEmail_ShouldReturnOk_If_ToEmailIsNotNull()
        {
            // Arrange
            var emailMessage = new EmailMessage("subject", "body", "from@email.com", "to@email.com");

            // Act
            var result = await _provider.SendEmail(emailMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
        }

        [Fact]
        public async Task SendAwsSesEmail_Should_ThrowException_If_Email_Throws()
        {
            // Arrange
            var emailMessage = new EmailMessage("subject", "body", "from@email.com", "to@email.com");
            _mockEmailService.Setup(_ => _.SendRawEmailAsync(It.IsAny<SendRawEmailRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _provider.SendEmail(emailMessage));
        }
    }
}