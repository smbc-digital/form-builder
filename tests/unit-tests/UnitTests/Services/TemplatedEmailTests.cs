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
        private readonly IEnumerable<ITemplatedEmailProvider> _mockTemplatedEmailProviders;

        public TemplatedEmailTests()
        {
            _mockTemplatedEmailProvider.Setup(_ => _.ProviderName).Returns("Notify");

            _mockTemplatedEmailProviders = new List<ITemplatedEmailProvider> { _mockTemplatedEmailProvider.Object };

            _mockSessionHelper.Setup(_ => _.GetBrowserSessionId()).Returns("sessionGuid");

            _templatedEmailService = new TemplatedEmailService(_mockTemplatedEmailProviders, _mockActionHelper.Object, _mockSessionHelper.Object, _mockDistributedCache.Object);
        }

        [Fact]
        public async Task Process_ShouldThrowException_IfSessionIsNull()
        {
            // Arrange
            _mockDistributedCache
                .Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns("");

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _templatedEmailService.ProcessTemplatedEmail(new List<IAction> { new UserEmail() }, "form"));
            Assert.Contains("TemplatedEmailService::Process: Session has expired", result.Message);
        }

        [Fact]
        public void Process_ShouldCallSendEmailAsync_ForTemplatedEmailType()
        {
            // Arrange
            var action = new ActionBuilder()
               .WithActionType(EActionType.TemplatedEmail)
               .WithProvider("Notify")
               .Build();

            _mockActionHelper
                .Setup(_ => _.GetEmailToAddresses(It.IsAny<IAction>(), It.IsAny<FormAnswers>()))
                .Returns("test@testemail.com");

            var formData = JsonConvert.SerializeObject(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(formData);

            // Act
            _templatedEmailService.ProcessTemplatedEmail(new List<IAction> { action }, "form");

            // Assert
            _mockTemplatedEmailProvider.Verify(_ => _.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>()), Times.Once);
        }

        [Fact]
        public void Process_ShouldCallSendEmailAsync_WithCaseReference()
        {
            // Arrange
            var action = new ActionBuilder()
               .WithActionType(EActionType.TemplatedEmail)
               .WithTo("test@abc.com")
               .WithTemplateId("123")
               .WithProvider("Notify")
               .WithCaseReference(true)
               .Build();

            _mockActionHelper.Setup(_ => _.GetEmailToAddresses(It.IsAny<IAction>(), It.IsAny<FormAnswers>()))
                .Returns("test@testemail.com");

            var cacheData = new FormAnswers
            {
                CaseReference = "test-ref",
                Path = "page-one",
                Pages = new List<PageAnswers>()
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "test"
                            }
                        },
                        PageSlug = "page-one"
                    }
                }
            };
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var personalisationSent = new Dictionary<string, dynamic>();

            _mockTemplatedEmailProvider
                .Setup(_ => _.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, string, Dictionary<string, dynamic>>((emailAddress, templateId, personalisation) => personalisationSent = personalisation);

            // Act
            _templatedEmailService.ProcessTemplatedEmail(new List<IAction> { action }, "form");

            // Assert
            Assert.Equal(personalisationSent["reference"], "test-ref");
            Assert.Single(personalisationSent);
        }

        [Fact]
        public void Process_ShouldCallSendEmailAsync_WithPersonalisation()
        {
            // Arrange
            var action = new ActionBuilder()
               .WithActionType(EActionType.TemplatedEmail)
               .WithTo("test@abc.com")
               .WithTemplateId("123")
               .WithProvider("Notify")
               .WithPersonalisation(new List<string> { "firstname" })
               .Build();

            _mockActionHelper.Setup(_ => _.GetEmailToAddresses(It.IsAny<IAction>(), It.IsAny<FormAnswers>()))
                .Returns("test@testemail.com");

            var cacheData = new FormAnswers
            {
                CaseReference = "test-ref",
                Path = "page-one",
                Pages = new List<PageAnswers>()
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "test"
                            }
                        },
                        PageSlug = "page-one"
                    }
                }
            };
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var personalisationSent = new Dictionary<string, dynamic>();
            _mockTemplatedEmailProvider
                .Setup(_ => _.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, string, Dictionary<string, dynamic>>((emailAddress, templateId, personalisation) => personalisationSent = personalisation);

            // Act
            _templatedEmailService.ProcessTemplatedEmail(new List<IAction> { action }, "form");

            // Assert
            Assert.Equal(personalisationSent["firstname"], "test");
            Assert.Single(personalisationSent);
        }
    }
}
