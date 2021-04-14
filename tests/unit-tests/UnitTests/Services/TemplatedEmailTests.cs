using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.TemplatedEmailProvider;
using form_builder.Services.TemplatedEmailService;
using form_builder_tests.Builders;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class TemplatedEmailTests
    {
        private readonly TemplatedEmailService _templatedEmailService;
        private readonly Mock<IActionHelper> _mockActionHelper = new();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new();
        private readonly Mock<ITemplatedEmailProvider> _mockTemplatedEmailProvider = new();
        private readonly Mock<IEnumerable<ITemplatedEmailProvider>> _mockTemplatedEmailProviders = new();

        public TemplatedEmailTests()
        {
            var providers = new List<ITemplatedEmailProvider> { _mockTemplatedEmailProvider.Object };
            _mockTemplatedEmailProviders.Setup(m => m.GetEnumerator()).Returns(() => providers.GetEnumerator());

            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("sessionGuid");

            var formData = JsonConvert.SerializeObject(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(formData);

            _templatedEmailService = new TemplatedEmailService(_mockTemplatedEmailProviders.Object, _mockActionHelper.Object, _mockSessionHelper.Object, _mockDistributedCache.Object);
        }

        [Fact]
        public async Task Process_ShouldThrowException_IfSessionIsNull()
        {
            // Arrange
            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("");

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _templatedEmailService.ProcessTemplatedEmail(new List<IAction> { new UserEmail() }));
            Assert.Contains("TemplatedEmailService::Process: Session has expired", result.Message);
        }

        [Fact]
        public void Process_ShouldCallSendEmailAsync_ForTemplatedEmailType()
        {
            // Arrange
            var action = new ActionBuilder()
               .WithActionType(EActionType.TemplatedEmail)
               .Build();

            _mockActionHelper.Setup(_ => _.GetEmailToAddresses(It.IsAny<IAction>(), It.IsAny<FormAnswers>()))
                .Returns("test@testemail.com");

            // Act
            _templatedEmailService.ProcessTemplatedEmail(new List<IAction> { action });

            // Assert
            _mockTemplatedEmailProvider.Verify(_ => _.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>()), Times.Once);
        }

        //verify if u have some personalisation that passed into the provider
        [Fact]
        public void Process_ShouldCallSendEmailAsync_IfPersonalisationIsValid()
        {
            // Arrange
            var action = new ActionBuilder()
               .WithActionType(EActionType.TemplatedEmail)
               .Build();

            _mockActionHelper.Setup(_ => _.GetEmailToAddresses(It.IsAny<IAction>(), It.IsAny<FormAnswers>()))
                .Returns("test@testemail.com");

            var personalisation = new Dictionary<string, dynamic>();

            _mockTemplatedEmailProvider
                .Setup(_ => _.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback(() => personalisation.Add("test", "test"));
            // Act
            _templatedEmailService.ProcessTemplatedEmail(new List<IAction> { action });

            // Assert
            _mockTemplatedEmailProvider.Verify(_ => _.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>()), Times.Once);

            Assert.Equal("test", "test");
        }
    }
}
