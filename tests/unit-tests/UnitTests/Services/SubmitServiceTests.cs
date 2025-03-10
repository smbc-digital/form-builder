using System.Dynamic;
using System.Net;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.PaymentHelpers;
using form_builder.Helpers.Submit;
using form_builder.Models;
using form_builder.Providers.ReferenceNumbers;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Submit;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.SubmitService;
using form_builder.TagParsers;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class SubmitServiceTests
    {
        private readonly SubmitService _service;
        private readonly Mock<IGateway> _mockGateway = new();
        private readonly Mock<IPageHelper> _mockPageHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockEnvironment = new();
        private readonly Mock<IOptions<SubmissionServiceConfiguration>> _mockIOptions = new();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new();
        private readonly Mock<ISchemaFactory> _mockSchemaFactory = new();
        private readonly Mock<IReferenceNumberProvider> _mockReferenceNumberProvider = new();
        private readonly Mock<ISubmitProvider> _mockSubmitProvider = new();
        private readonly Mock<ISubmitHelper> _mockSubmitHelper = new();
        private readonly Mock<IPaymentHelper> _mockPaymentHelper = new();
        private readonly IEnumerable<ISubmitProvider> _submitProviders;
        private readonly Mock<IEnumerable<ITagParser>> _mockTagParsers = new();
        private readonly Mock<ITagParser> _tagParser = new();
        private readonly Mock<ILogger<SubmitService>> _mockLogger = new();

        public SubmitServiceTests()
        {
            _tagParser.Setup(_ => _.Parse(It.IsAny<Page>(), It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()))
                .ReturnsAsync(new Page());
            var tagParserItems = new List<ITagParser> { _tagParser.Object };
            _mockTagParsers.Setup(m => m.GetEnumerator()).Returns(() => tagParserItems.GetEnumerator());

            _mockEnvironment
                .Setup(_ => _.EnvironmentName)
                .Returns("local");

            _mockIOptions
                .Setup(_ => _.Value)
                .Returns(new SubmissionServiceConfiguration
                {
                    FakeSubmission = false
                });

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(new FormSchema
                {
                    GenerateReferenceNumber = false
                });

            _mockReferenceNumberProvider
                .Setup(_ => _.GetReference(It.IsAny<string>(), It.IsAny<int>()))
                .Returns("TEST123456");

            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()))
                .ReturnsAsync(new PaymentInformation { Settings = new Settings { Amount = "10.00" } });

            _mockSubmitProvider
                .Setup(_ => _.ProviderName).Returns("AuthHeader");

            _submitProviders = new List<ISubmitProvider>
            {
                _mockSubmitProvider.Object
            };

            _service = new SubmitService(
                _mockGateway.Object,
                _mockPageHelper.Object,
                _mockEnvironment.Object,
                _mockIOptions.Object,
                _mockDistributedCache.Object,
                _mockSchemaFactory.Object,
                _mockReferenceNumberProvider.Object,
                _submitProviders,
                _mockPaymentHelper.Object,
                _mockSubmitHelper.Object,
                _mockTagParsers.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ProcessSubmission_Application_ShouldThrowApplicationException_WhenNoSubmitUrlSpecified()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
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

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act
            var result = await Assert.ThrowsAsync<NullReferenceException>(() => _service.ProcessSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.Equal("Page model::GetSubmitFormEndpoint, No postUrl supplied for submit form", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task ProcessSubmission_Application_ShouldCatchException_WhenGatewayCallThrowsException()
        {
            // Arrange
            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.Environment.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("testUrl")
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("error"));

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _service.PaymentSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            Assert.StartsWith("error", result.Message);
        }

        [Fact]
        public async Task ProcessSubmission_ShouldCallProvider_WithFormData()
        {
            // Arrange
            var questionId = "testQuestion";
            var callbackValue = new ExpandoObject() as IDictionary<string, object>;

            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .Build();

            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            _mockSubmitProvider
            .Setup(_ => _.PostAsync(It.IsAny<MappingEntity>(), It.IsAny<SubmitSlug>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            // Act
            await _service.ProcessSubmission(new MappingEntity { Data = new ExpandoObject(), BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", "123454");

            // Assert
            _mockSubmitProvider.Verify(_ => _.PostAsync(It.IsAny<MappingEntity>(), It.IsAny<SubmitSlug>()), Times.Once);

            Assert.NotNull(callbackValue);
        }

        [Fact]
        public async Task ProcessSubmission_ShouldCallTagParsers()
        {
            // Arrange
            var questionId = "testQuestion";

            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .Build();

            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            _mockSubmitProvider
                .Setup(_ => _.PostAsync(It.IsAny<MappingEntity>(), It.IsAny<SubmitSlug>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            // Act
            await _service.ProcessSubmission(new MappingEntity { Data = new ExpandoObject(), BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", "123454");

            // Assert
            _tagParser.Verify(_ => _.Parse(It.IsAny<Page>(), It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()), Times.Once);
        }

        [Fact]
        public async Task PreProcessSubmission_ShouldCallGateway_CreateReferenceAndSave()
        {
            // Arrange
            var schema = new FormSchemaBuilder()
                .WithGeneratedReference("CaseReference", "TEST")
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.SaveCaseReference(It.IsAny<string>(), It.IsAny<string>(), true, It.IsAny<string>()))
                .Verifiable();

            // Act
            await _service.PreProcessSubmission("form", "123454");

            // Assert
            _mockReferenceNumberProvider.Verify(_ => _.GetReference(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveCaseReference(It.IsAny<string>(), It.IsAny<string>(), true, "CaseReference"), Times.Once);
            _mockPageHelper.Verify(_ => _.SavePaymentAmount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task PreProcessSubmission_SavePaymentAmount()
        {
            // Arrange
            var schema = new FormSchemaBuilder()
                .WithSavePaymentAmount("paymentAmount")
                .WithBaseUrl("form")
                .Build();

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.SaveCaseReference(It.IsAny<string>(), It.IsAny<string>(), true, It.IsAny<string>()))
                .Verifiable();

            // Act
            await _service.PreProcessSubmission("form", "123454");

            // Assert
            _mockPageHelper.Verify(_ => _.SavePaymentAmount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessSubmission_ShouldReturn_GeneratedReference_IfGeneratedReferenceIsTrue()
        {
            // Arrange
            var callbackValue = new ExpandoObject() as IDictionary<string, object>;

            var schema = new FormSchemaBuilder()
                .WithGeneratedReference("CaseReference", "TEST")
                .Build();

            var questionId = "testQuestion";
            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .Build();

            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };
            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                })
                .Callback<string, object>((x, y) => callbackValue = (ExpandoObject)y);

            var json = JsonConvert.SerializeObject(new FormAnswers
            {
                CaseReference = "TEST123456",
                AdditionalFormData = new Dictionary<string, object>()
                {
                    { "CaseReference", "TEST123456" }
                }
            });

            _mockDistributedCache
                .Setup(_ => _.GetString(It.IsAny<string>())).Returns(json);

            _mockSubmitProvider
            .Setup(_ => _.PostAsync(It.IsAny<MappingEntity>(), It.IsAny<SubmitSlug>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            // Act
            var result = await _service.ProcessSubmission(new MappingEntity { Data = new ExpandoObject(), BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", "123454");

            // Assert
            Assert.Equal("TEST123456", result);
        }

        [Fact]
        public async Task ProcessSubmission_Application_ShouldThrowApplicationException_WhenProviderResponse_IsNotOk()
        {
            // Arrange
            var element = new ElementBuilder()
                    .WithType(EElementType.H1)
                    .WithQuestionId("test-id")
                    .WithPropertyText("test-text")
                    .Build();

            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSubmitProvider
                .Setup(_ => _.PostAsync(It.IsAny<MappingEntity>(), It.IsAny<SubmitSlug>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith("SubmitService::ProcessSubmission, An exception has occurred while attempting to call ", result.Message);
            _mockSubmitProvider.Verify(_ => _.PostAsync(It.IsAny<MappingEntity>(), It.IsAny<SubmitSlug>()), Times.Once);
        }

        [Fact]
        public async Task PaymentSubmission_ShouldCallGateway_AndReturn_Reference()
        {
            // Arrange
            var guid = Guid.NewGuid();

            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("testUrl")
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("\"1234456\"")
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act
            var result = await _service.PaymentSubmission((new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }), "form", guid.ToString());

            // Assert
            Assert.IsType<string>(result);

            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()));
        }

        [Fact]
        public async Task PaymentSubmission_ShouldThrowApplicationException_WhenNotOkResponse()
        {
            // Arrange
            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .WithPageSlug("testUrl")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.PaymentSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith("SubmitService::PaymentSubmission, An exception has occurred while attempting to call ", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PaymentSubmission_ShouldThrowApplicationException_WhenNoContentFromGateway()
        {
            // Arrange
            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };
            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("testUrl")
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = null
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.PaymentSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith("SubmitService::PaymentSubmission, Gateway www.location.com responded with empty reference", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PaymentSubmission_ShouldThrowApplicationException_WhenGatewayResponseContent_IsEmpty()
        {
            // Arrange
            var postUrl = "www.post.url";
            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .WithPageSlug(postUrl)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.PaymentSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith("SubmitService::PaymentSubmission, Gateway", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ProcessSubmission_ShouldCall_SubmitHelper_ConfirmBookings()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithAppointmentType(new AppointmentType { AppointmentId = Guid.Parse("37588e67-9852-4713-9df5-0eb94e320675"), Environment = "local" })
                .WithBookingProvider("testBookingProvider")
                .WithAutoConfirm(true)
                .Build();

            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var formAnswers = new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = "booking-reserved-booking-id",
                                Response = "93dd24cd-cea5-40e7-b72a-a6b4757786ba"
                            }
                        }
                    }
                }
            };

            var _mappingEntity =
            new MappingEntityBuilder()
                .WithBaseForm(schema)
                .WithFormAnswers(formAnswers)
                .WithData(new ExpandoObject())
                .Build();

            _mockSubmitProvider
            .Setup(_ => _.PostAsync(It.IsAny<MappingEntity>(), It.IsAny<SubmitSlug>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            // Act
            await _service.ProcessSubmission(_mappingEntity, "form", "123454");

            // Assert
            _mockSubmitHelper.Verify(_ => _.ConfirmBookings(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RedirectSubmission_ShouldCallGateway_AndReturn_Reference()
        {
            // Arrange
            var guid = Guid.NewGuid();

            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com", RedirectUrl = "www.redirect.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("testUrl")
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("\"1234456\"")
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act
            await _service.RedirectSubmission((new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }), "form", guid.ToString());

            // Assert
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()));
        }

        [Fact]
        public async Task RedirectSubmission_ShouldThrowApplicationException_WhenNotOkResponse()
        {
            // Arrange
            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com", RedirectUrl = "www.redirect.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .WithPageSlug("testUrl")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.RedirectSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith("SubmitService::RedirectSubmission, An exception has occurred while attempting to call ", result.Message);
        }

        [Fact]
        public async Task RedirectSubmission_ShouldThrowApplicationException_WhenGatewayResponseContent_IsEmpty()
        {
            // Arrange
            var postUrl = "www.post.url";
            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com", RedirectUrl = "www.redirect.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .WithPageSlug(postUrl)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.RedirectSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith("SubmitService::RedirectSubmission, Gateway", result.Message);
        }

        [Fact]
        public async Task RedirectSubmission_TagParser_ShouldCallParseString()
        {
            // Arrange
            var postUrl = "www.post.url";
            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com", RedirectUrl = "www.redirect.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .WithPageSlug(postUrl)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("\"1234456\"")
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(page);

            // Act
            var result = await _service.RedirectSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", "");

            // Assert
            _tagParser.Verify(_ => _.ParseString(It.IsAny<string>(), It.IsAny<FormAnswers>()), Times.Once);
        }

        [Fact]
        public async Task ProcessWithoutSubmission_ShouldCall_SubmitHelper_ConfirmBookings()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithAppointmentType(new AppointmentType { AppointmentId = Guid.Parse("37588e67-9852-4713-9df5-0eb94e320675"), Environment = "local" })
                .WithBookingProvider("testBookingProvider")
                .WithAutoConfirm(true)
                .Build();

            var submitSlug = new SubmitSlug { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var formAnswers = new FormAnswers
            {
                Path = "page-one",
                CaseReference = "caseReference",
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "booking-reserved-booking-id",
                                Response = "93dd24cd-cea5-40e7-b72a-a6b4757786ba"
                            }
                        }
                    }
                }
            };

            var mappingEntity = new MappingEntityBuilder()
                .WithBaseForm(schema)
                .WithFormAnswers(formAnswers)
                .WithData(new ExpandoObject())
                .Build();

            _mockSubmitProvider
                .Setup(_ => _.PostAsync(It.IsAny<MappingEntity>(), It.IsAny<SubmitSlug>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            _mockDistributedCache
                .Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(formAnswers));

            // Act
            await _service.ProcessWithoutSubmission(mappingEntity, "form", "123454");

            // Assert
            _mockSubmitHelper.Verify(_ => _.ConfirmBookings(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
