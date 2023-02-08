using Moq;
using form_builder.Enum;
using form_builder.Helpers.EmailHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.EmailProvider;
using form_builder.Services.DocumentService;
using form_builder.Services.DocumentService.Entities;
using form_builder.Services.EmailSubmitService;
using form_builder.Services.MappingService;
using form_builder.Providers.ReferenceNumbers;
using form_builder.Services.MappingService.Entities;
using form_builder.Configuration;
using form_builder.Builders;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class EmailSubmitServiceTests
    {
        private readonly Mock<IMappingService> _mappingService = new();
        private readonly Mock<IEmailHelper> _emailHelper = new();
        private readonly Mock<ISessionHelper> _sessionHelper = new();
        private readonly Mock<IEmailProvider> _emailProvider = new();
        private readonly Mock<IDocumentSummaryService> _documentSummaryService = new();
        private readonly Mock<IReferenceNumberProvider> _referenceNumberProvider = new();
        private readonly Mock<IPageHelper> _pageHelper = new();
        private readonly EmailSubmitService _emailSubmitService;

        public EmailSubmitServiceTests()
        {
            _emailSubmitService = new EmailSubmitService(
                _mappingService.Object,
                _emailHelper.Object,
                _sessionHelper.Object,
                _pageHelper.Object,
                _emailProvider.Object,
                _referenceNumberProvider.Object,
                _documentSummaryService.Object
              );
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
                .ReturnsAsync(new EmailConfiguration
                {
                    FormName = new List<string> { "form" },
                    Subject = "test",
                    Recipient = new List<string> { "google" }
                });

            _referenceNumberProvider
                 .Setup(_ => _.GetReference(It.IsAny<string>(), 8))
                 .Returns("12345678");

            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndEmail)
                .WithPageSlug(null)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Act
            var result = await _emailSubmitService.EmailSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", "sessionGuid");

            // Assert
            _documentSummaryService.Verify(_ => _.GenerateDocument(It.IsAny<DocumentSummaryEntity>()), Times.Once);
            _emailHelper.Verify(_ => _.GetEmailInformation(It.IsAny<string>()), Times.Once);
            _emailProvider.Verify(_ => _.SendEmail(It.IsAny<EmailMessage>()), Times.Once);
        }
    }
}
