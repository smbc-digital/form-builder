using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Helpers.DocumentCreation;
using form_builder.Helpers.EmailHelpers;
using form_builder.Models;
using form_builder.Providers.DocumentCreation;
using form_builder.Services.DocumentService;
using form_builder.Services.DocumentService.Entities;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class DocumentSummaryServiceTests
    {
        private readonly Mock<IDocumentCreationHelper> _mockDocumentCreationHelper = new();
        private readonly Mock<ISchemaFactory> _mockSchemaFactory = new();
        private readonly Mock<IDocumentCreation> _mockProvider = new();
		private readonly Mock<IEmailHelper> _mockEmailHelper = new();
		private readonly DocumentSummaryService _documentSummaryService;
        private readonly FormSchema _formSchema;
        private readonly FormAnswers _formAnswers;

        public DocumentSummaryServiceTests()
        {
            _mockProvider.Setup(_ => _.DocumentType).Returns(EDocumentType.Txt);
            _mockProvider.Setup(_ => _.Priority).Returns(EProviderPriority.High);
            var providers = new List<IDocumentCreation> { _mockProvider.Object };

            _formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        PageSlug = "page-1",
                        Answers = new List<Answers>()
                    }
                }
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("success")
                .Build();

            var page1 = new PageBuilder()
                .WithPageSlug("page-1")
                .WithBehaviour(behaviour)
                .Build();

            var page2 = new PageBuilder()
                .WithPageSlug("success")
                .Build();

            _formSchema = new FormSchemaBuilder()
                .WithPage(page1)
                .WithPage(page2)
                .WithFirstPageSlug("page-1")
                .Build();

            _mockSchemaFactory.Setup(_ => _.TransformPage(page1, It.IsAny<FormAnswers>())).ReturnsAsync(page1);
            _mockSchemaFactory.Setup(_ => _.TransformPage(page2, It.IsAny<FormAnswers>())).ReturnsAsync(page2);

            _mockDocumentCreationHelper
                .Setup(_ => _.GenerateQuestionAndAnswersList(It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()))
                .ReturnsAsync(new List<string>());

            _mockProvider.Setup(_ => _.CreateDocument(It.IsAny<List<string>>())).Returns(new byte[1]);

            _documentSummaryService = new DocumentSummaryService(_mockDocumentCreationHelper.Object, providers, _mockSchemaFactory.Object, _mockEmailHelper.Object);
        }

        [Fact]
        public async Task GenerateDocument_ShouldCallDocumentCreationHelper_WhenDocumentTypeIsTxt()
        {
            // Act
            await _documentSummaryService.GenerateDocument(new DocumentSummaryEntity { FormSchema = _formSchema, DocumentType = EDocumentType.Txt, PreviousAnswers = _formAnswers });

            // Assert
            _mockDocumentCreationHelper.Verify(_ => _.GenerateQuestionAndAnswersList(It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()), Times.Once);
        }

        [Fact]
        public async Task GenerateDocument_ShouldCallTextfileProvider_WhenDocumentTypeIsTxt()
        {
            // Act
            await _documentSummaryService.GenerateDocument(new DocumentSummaryEntity { FormSchema = _formSchema, DocumentType = EDocumentType.Txt, PreviousAnswers = _formAnswers });

            // Assert
            _mockProvider.Verify(_ => _.CreateDocument(It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task GenerateDocument_ShouldThrow_WhenDocumentTypeIsUnknown()
        {
            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() =>
                _documentSummaryService.GenerateDocument(new DocumentSummaryEntity
                { FormSchema = _formSchema, DocumentType = EDocumentType.Unknown, PreviousAnswers = _formAnswers }));

            Assert.Equal("DocumentSummaryService::GenerateDocument, Unknown Document type request for Summary", result.Message);
        }
    }
}
