using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Models.Elements;
using form_builder.Providers.EmailProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.EmailService;
using form_builder_tests.Builders;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class EmailServiceTests
    {
        private readonly EmailService _emailService;
        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IEmailProvider> _mockEmailProvider = new Mock<IEmailProvider>();
        private readonly Mock<IEnumerable<IEmailProvider>> _mockEmailProviders = new Mock<IEnumerable<IEmailProvider>>();
        private readonly Mock<IActionHelper> _mockActionHelper = new Mock<IActionHelper>();

        public EmailServiceTests()
        {

            var emailProviderItems = new List<IEmailProvider> { _mockEmailProvider.Object };
            _mockEmailProviders.Setup(m => m.GetEnumerator()).Returns(() => emailProviderItems.GetEnumerator());

            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("sessionGuid");

            var formData = JsonConvert.SerializeObject(new FormAnswers
            { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(formData);

            _emailService = new EmailService(
              _mockSessionHelper.Object,
              _mockDistributedCache.Object,
              _mockEmailProviders.Object,
              _mockActionHelper.Object);
        }

        [Fact]
        public async Task Process_ShouldCall_SessionHelper()
        {
            // Arrange
            var action = new ActionBuilder()
                .WithActionType(EActionType.UserEmail)
                .Build();
            
            _mockActionHelper.Setup(_ => _.GetEmailToAddresses(It.IsAny<IAction>(), It.IsAny<FormAnswers>()))
                .Returns("test@testemail.com");

            _mockEmailProvider.Setup(_ => _.SendEmail(It.IsAny<EmailMessage>())).ReturnsAsync(HttpStatusCode.OK);

            // Act
            await _emailService.Process(new List<IAction>{ action });

            // Assert
            _mockSessionHelper.Verify(_ => _.GetSessionGuid(), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldThrowException_IfSessionIsNull()
        {
            // Arrange
            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("");

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _emailService.Process(new List<IAction>{ new UserEmail() }));
            Assert.Contains("EmailService::Process: Session has expired", result.Message);
        }

        [Fact]
        public async Task Process_ShouldCall_DistributedCache()
        {
            // Arrange
            var action = new ActionBuilder()
                .WithActionType(EActionType.UserEmail)
                .Build();

            _mockActionHelper.Setup(_ => _.GetEmailToAddresses(It.IsAny<IAction>(), It.IsAny<FormAnswers>()))
                .Returns("test@testemail.com");

            // Act
            await _emailService.Process(new List<IAction>{ action });

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldCall_ActionHelper_IfTypeIsUserEmail()
        {
            // Arrange
              var action = new ActionBuilder()
                .WithActionType(EActionType.UserEmail)
                .Build();

            // Act
            await _emailService.Process(new List<IAction>{ action });

            // Assert
            _mockActionHelper.Verify(_ => _.GetEmailToAddresses(It.IsAny<IAction>(), It.IsAny<FormAnswers>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldCall_EmailProvider_IfTypeIsUserEmail()
        {
            // Arrange
             var action = new ActionBuilder()
                .WithActionType(EActionType.UserEmail)
                .Build();

            // Act
            await _emailService.Process(new List<IAction>{ action });

            // Assert
            _mockEmailProvider.Verify(_ => _.SendEmail(It.IsAny<EmailMessage>()), Times.Once);
        }
    }
}
