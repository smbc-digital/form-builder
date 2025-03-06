using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.EmailHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.EmailProvider;
using form_builder.Providers.ReferenceNumbers;
using form_builder.Services.DocumentService;
using form_builder.Services.DocumentService.Entities;
using form_builder.Services.EmailSubmitService;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.TagParsers;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using System.Net;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class EmailSubmitServiceTests
    {
        private readonly Mock<IMappingService> _mappingService = new();
        private readonly Mock<IEmailHelper> _mockEmailHelper = new();
        private readonly Mock<IElementMapper> _mockElementMapper = new();
        private readonly Mock<ISessionHelper> _sessionHelper = new();
        private readonly Mock<IEmailProvider> _emailProvider = new();
        private readonly Mock<IDocumentSummaryService> _documentSummaryService = new();
        private readonly Mock<IReferenceNumberProvider> _referenceNumberProvider = new();
        private readonly Mock<IPageHelper> _pageHelper = new();
        private readonly EmailSubmitService _emailSubmitService;
        private readonly Mock<IEnumerable<ITagParser>> _mockTagParsers = new();
        private readonly Mock<ITagParser> _tagParser = new();
        private readonly Mock<ILogger<EmailSubmitService>> _mockLogger = new();
        private readonly Mock<IGateway> _mockGateway = new();
        private readonly Mock<IWebHostEnvironment> _hostingEnvironment = new();
        private readonly Mock<IOptions<PowerAutomateConfiguration>> _mockPowerAutomateConfiguration = new();

        public EmailSubmitServiceTests()
        {
            _tagParser
                .Setup(_ => _.ParseString(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns("{'AttachPdf':true,'Body':null,'FormName':['jw-roast-preference'],'Recipient':['jonathon.warwick@stockport.gov.uk'],'Sender':'noreply@stockport.gov.uk','Subject':'JW Roast Preference: {{QUESTION:firrstName}} {{QUESTION:favouriteyFood}}'}");

            var tagParserItems = new List<ITagParser> { _tagParser.Object };

            _mockTagParsers
                .Setup(m => m.GetEnumerator())
                .Returns(() => tagParserItems.GetEnumerator());

            _sessionHelper
                 .Setup(_ => _.GetBrowserSessionId())
                 .Returns("123454");

            _mappingService
                .Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .ReturnsAsync(new MappingEntity { BaseForm = new FormSchema() });

            _documentSummaryService
                .Setup(_ => _.GenerateDocument(It.IsAny<DocumentSummaryEntity>()))
                .ReturnsAsync(new byte[] { 0x1a, 0x0f, 0x00 });

            _referenceNumberProvider
                 .Setup(_ => _.GetReference(It.IsAny<string>(), 8))
                 .Returns("12345678");

            _emailProvider
                .Setup(_ => _.SendEmail(It.IsAny<EmailMessage>()))
                .ReturnsAsync(HttpStatusCode.OK);

            _mockEmailHelper
                .Setup(_ => _.GetEmailInformation(It.IsAny<string>()))
                .ReturnsAsync(new EmailConfiguration
                {
                    FormName = new List<string> { "form" },
                    Subject = "test",
                    Recipient = new List<string> { "google" }
                });

            _emailSubmitService = new EmailSubmitService(
                _mappingService.Object,
                _mockEmailHelper.Object,
                _mockElementMapper.Object,
                _sessionHelper.Object,
                _pageHelper.Object,
                _emailProvider.Object,
                _referenceNumberProvider.Object,
                _documentSummaryService.Object,
                _mockTagParsers.Object,
                _mockLogger.Object,
                _mockGateway.Object,
                _hostingEnvironment.Object,
                _mockPowerAutomateConfiguration.Object
              );
        }

        [Fact]
        public async Task Submit_ShouldCallMapping_And_SubmitService()
        {
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

            var paConfig = new PowerAutomateConfiguration();
            _mockPowerAutomateConfiguration.Setup(_ => _.Value).Returns(paConfig);

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("Okay")
                });

            // Act
            var result = await _emailSubmitService.EmailSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", "sessionGuid");

            // Assert
            _documentSummaryService.Verify(_ => _.GenerateDocument(It.IsAny<DocumentSummaryEntity>()), Times.Once);
            _mockEmailHelper.Verify(_ => _.GetEmailInformation(It.IsAny<string>()), Times.Once);
            _emailProvider.Verify(_ => _.SendEmail(It.IsAny<EmailMessage>()), Times.Once);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<PowerAutomateDetails>()), Times.Once);
        }

        [Fact]
        public async Task Submit_ShouldSubmitAndEmail_ThrowExceptionAndRemoveVariable()
        {
            // Arrange
            string actualSubject = "";
            string expectedSubject = "Subject: ";

            EmailConfiguration emailConfig = new()
            {
                FormName = new List<string> { "form" },
                Subject = "Subject: {{QUESTION: test-id}}",
                Recipient = new List<string> { "test@stockport.com" }
            };

            _mockEmailHelper
                .Setup(_ => _.GetEmailInformation(It.IsAny<string>()))
                .ReturnsAsync(emailConfig);

            _emailProvider
                .Setup(_ => _.SendEmail(It.IsAny<EmailMessage>()))
                .Callback<EmailMessage>(emailMessageSent => actualSubject = emailMessageSent.Subject)
                .ReturnsAsync(HttpStatusCode.OK);

            _tagParser
                .Setup(_ => _.ParseString(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Throws(new Exception());

            // Act & Assert
            await _emailSubmitService.EmailSubmission(new MappingEntity { BaseForm = new FormSchema(), FormAnswers = new FormAnswers() }, "form", "sessionGuid");
            Assert.Equal(expectedSubject, actualSubject);
        }

        [Fact]
        public async Task Submit_ShouldCallElementMapper_ToGetListOf_FileUploads()
        {
            // Arrange
            var emailMessage = new EmailMessage("subject", "body", "email@stockport.gov.uk", "email@stockport.gov.uk");

            var fileUploadElement = new ElementBuilder()
               .WithLabel("File upload")
               .WithQuestionId("questionIDOne")
               .WithType(EElementType.FileUpload)
               .Build();

            var data = new MappingEntity
            {
                FormAnswers = new FormAnswers
                {
                    Path = "page-one",
                    FormName = "form",
                    Pages = new List<PageAnswers>
                    {
                        new()
                        {
                            Answers = new List<Answers>
                            {
                                    new() { QuestionId = $"questionIDOne{FileUploadConstants.SUFFIX}", Response = new List<FileUploadModel>() },
                                    new() { QuestionId = $"questionIDTwo{FileUploadConstants.SUFFIX}", Response = new List<FileUploadModel>() }
                            }
                        }
                    }
                },
                BaseForm = new FormSchema
                {
                    Pages = new List<Page>
                    {
                        new() { Elements = new List<IElement> { fileUploadElement } }
                    }
                }
            };

            await _emailSubmitService.EmailSubmission(data, "form", "sessionGuid");
            _mockElementMapper.Verify(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()), Times.Once);
        }
    }
}
