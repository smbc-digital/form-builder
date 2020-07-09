using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Providers.EmailProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.ActionService;
using form_builder_tests.Builders;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class ActionServiceTests
    {
        private readonly ActionService _actionService;
        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IEmailProvider> _mockEmailProvider = new Mock<IEmailProvider>();
        private readonly Mock<IActionHelper> _mockActionHelper = new Mock<IActionHelper>();

        private static readonly Element Element = new ElementBuilder()
            .WithType(EElementType.H2)
            .Build();

        private static readonly Page Page = new PageBuilder()
            .WithElement(Element)
            .WithPageSlug("success")
            .Build();

        private static readonly FormSchema FormSchema = new FormSchemaBuilder()
            .WithDocumentDownload(true)
            .WithDocumentType(EDocumentType.Txt)
            .WithStartPageSlug("page-one")
            .WithBaseUrl("base-test")
            .WithPage(Page)
            .WithFormActions(new FormAction
            {
                Type = EFormActionType.UserEmail,
                Properties = new BaseActionProperty()
            })
            .Build();

        public ActionServiceTests()
        {
            _actionService = new ActionService(
                _mockSessionHelper.Object, 
                _mockDistributedCache.Object, 
                _mockEmailProvider.Object,
                _mockActionHelper.Object);

            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("sessionGuid");

            var formData = JsonConvert.SerializeObject(new FormAnswers
                { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(formData);
        }

        [Fact]
        public async Task Process_ShouldCall_SessionHelper()
        {
            // Arrange
            _mockActionHelper.Setup(_ => _.GetEmailToAddresses(It.IsAny<FormAction>(), It.IsAny<FormAnswers>()))
                .Returns("test@testemail.com");

            _mockEmailProvider.Setup(_ => _.SendAwsSesEmail(It.IsAny<EmailMessage>())).ReturnsAsync(HttpStatusCode.OK);

            // Act
            await _actionService.Process(FormSchema);

            // Assert
            _mockSessionHelper.Verify(_ => _.GetSessionGuid(), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldThrowException_IfSessionIsNull()
        {
            // Arrange
            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("");

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _actionService.Process(FormSchema));
            Assert.Contains("ActionService::Process: Session has expired", result.Message);
        }

        [Fact]
        public async Task Process_ShouldCall_DistributedCache()
        {
            // Act
            await _actionService.Process(FormSchema);

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldCall_ActionHelper_IfTypeIsUserEmail()
        {
            // Act
            await _actionService.Process(FormSchema);

            // Assert
            _mockActionHelper.Verify(_ => _.GetEmailToAddresses(It.IsAny<FormAction>(), It.IsAny<FormAnswers>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldCall_EmailProvider_IfTypeIsUserEmail()
        {
            // Act
            await _actionService.Process(FormSchema);

            // Assert
            _mockEmailProvider.Verify(_ => _.SendAwsSesEmail(It.IsAny<EmailMessage>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldNotCall_ActionHelper_IfTypeIsUnknown()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H2)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("success")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithStartPageSlug("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .WithFormActions(new FormAction
                {
                    Properties = new BaseActionProperty(),
                    Type = EFormActionType.Unknown
                })
                .Build();

            // Act
            await _actionService.Process(formSchema);

            // Assert
            _mockActionHelper.Verify(_ => _.GetEmailToAddresses(It.IsAny<FormAction>(), It.IsAny<FormAnswers>()), Times.Never);
        }

        [Fact]
        public async Task Process_ShouldNotCall_EmailProvider_IfTypeIsUnknown()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H2)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("success")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithStartPageSlug("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .WithFormActions(new FormAction
                {
                    Properties = new BaseActionProperty(),
                    Type = EFormActionType.Unknown
                })
                .Build();

            // Act
            await _actionService.Process(formSchema);

            // Assert
            _mockEmailProvider.Verify(_ => _.SendAwsSesEmail(It.IsAny<EmailMessage>()), Times.Never);
        }
    }
}
