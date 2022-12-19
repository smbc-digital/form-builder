using form_builder.Enum;
using form_builder.Configuration;
using form_builder.Helpers.EmailHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.EmailProvider;
using form_builder.Services.DocumentService;
using form_builder.Services.DocumentService.Entities;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.SubmitService;
using form_builder.Workflows.EmailWorkflow;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Workflows
{
    public class EmailWorkflowTests
    {

        private readonly Mock<IMappingService>_mappingService = new();
        private readonly Mock<IEmailHelper> _emailHelper = new();
        private readonly Mock<ISessionHelper>_sessionHelper = new();
        private readonly Mock<IEmailProvider> _emailProvider = new();
        private readonly Mock<IDocumentSummaryService> _documentSummaryService=new();
        private readonly EmailWorkflow _emailWorkflow;
        public EmailWorkflowTests()
        {
            _emailWorkflow = new EmailWorkflow
            (
                _mappingService.Object,
                _emailHelper.Object,
                _sessionHelper.Object,
                _emailProvider.Object,
                _documentSummaryService.Object
            );
        }


        [Fact]
        public async Task Submit_ShouldThrowApplicationException_WhenNoSessionGuid()
        {
            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _emailWorkflow.Submit("form"));

            // Assert
            Assert.Equal("A Session GUID was not provided.", result.Message);
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _documentSummaryService.Verify(_ => _.GenerateDocument(It.IsAny<DocumentSummaryEntity>()), Times.Never);
            _emailProvider.Verify(_ => _.SendEmail(It.IsAny<EmailMessage>()), Times.Never);
        }

        [Fact]
        public async Task Submit_ShouldCallMapping_And_SubmitService()
        {
            // Arrange
            _sessionHelper
                .Setup(_ => _.GetSessionGuid())
                .Returns("123454");
            _mappingService
                .Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MappingEntity { BaseForm = new FormSchema() });
            _documentSummaryService
                .Setup(_ => _.GenerateDocument(It.IsAny<DocumentSummaryEntity>()))
                .ReturnsAsync(new byte[] { 0x1a, 0x0f, 0x00 });

            _emailProvider
                .Setup(_ => _.SendEmail(It.IsAny<EmailMessage>()))
                .ReturnsAsync(System.Net.HttpStatusCode.OK);

            _emailHelper
                .Setup(_ => _.GetEmailInformation(It.IsAny<string>()))
                .ReturnsAsync(new EmailConfig { FormName = new List<string> { "form" }, Subject = "test", To = new List<string> { "google" } });

            // Act
            await _emailWorkflow.Submit("form");

            // Assert
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _documentSummaryService.Verify(_ => _.GenerateDocument(It.IsAny<DocumentSummaryEntity>()), Times.Once);
            _emailHelper.Verify(_ => _.GetEmailInformation(It.IsAny<string>()), Times.Once);
            _emailProvider.Verify(_ => _.SendEmail(It.IsAny<EmailMessage>()), Times.Once);
        }

    }
}
