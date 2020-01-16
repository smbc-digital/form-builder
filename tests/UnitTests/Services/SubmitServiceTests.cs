using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.SubmitService.Entities;
using form_builder.Services.SubmtiService;
using form_builder_tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Gateways.ComplimentsComplaintsServiceGateway;
using Xunit;
using System.Dynamic;

namespace form_builder_tests.UnitTests.Services
{
    public class SubmitServiceTests
    {
        private readonly SubmitService _service;
        private readonly Mock<ILogger<SubmitService>> _mockLogger = new Mock<ILogger<SubmitService>>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistrubutedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<ISchemaProvider> _mockSchemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IGateway> _mockGateway = new Mock<IGateway>();
        private readonly Mock<IComplimentsComplaintsServiceGateway> _mockComplimentsComplaintsServiceGateway = new Mock<IComplimentsComplaintsServiceGateway>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<ISessionHelper> _sessionHelper = new Mock<ISessionHelper>();

        public SubmitServiceTests()
        {
            var cacheData = new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>()
                {
                    new PageAnswers
                    {
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = $"test",
                                Response = "test street"
                            }
                        },
                        PageSlug = "page-one"
                    }
                }
            };
            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(Newtonsoft.Json.JsonConvert.SerializeObject(cacheData));

            _service = new SubmitService(_mockLogger.Object, _mockDistrubutedCache.Object, _mockSchemaProvider.Object, _mockGateway.Object, _mockComplimentsComplaintsServiceGateway.Object, _pageHelper.Object, _sessionHelper.Object);
        }


        [Fact]
        public async Task ProcessSubmission_Application_ShouldThrowApplicationException_WhenNoSessionGuid()
        {
            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessSubmission("form"));

            // Assert
            Assert.Equal("A Session GUID was not provided.", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }


        [Fact]
        public async Task ProcessSubmission_Application_ShoudlThrowApplicationException_WhenNoSubmitUrlSpecified()
        {
            // Arrange
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123454");

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

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            var result = await Assert.ThrowsAsync<NullReferenceException>(() => _service.ProcessSubmission("form"));

            // Assert
            Assert.Equal("HomeController, Submit: No postUrl supplied for submit form", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }


        [Fact]
        public async Task ProcessSubmission_Applicaton_ShouldCatchException_WhenGatewayCallThrowsException()
        {
            // Arrange
            var guid = Guid.NewGuid();
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123454");

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("testUrl")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ProcessSubmission("form"));
        }

        [Fact]
        public async Task Submit_ShouldCallGateway_WithFormData()
        {
            // Arrange
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123454");
            var questionId = "testQuestion";
            var questionResponse = "testResponse";
            var callbackValue = new ExpandoObject() as IDictionary<string, object>;
            var cacheData = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        PageSlug = "page-one",
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                    QuestionId = questionId,
                                    Response = questionResponse
                            }
                        }
                    }
                },
                Path = "page-one"
            };

            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .Build();

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("testUrl")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                })
                .Callback<string, object>((x, y) => callbackValue = (ExpandoObject)y);
            // Act
            await _service.ProcessSubmission("form");

            // Assert
            _mockDistrubutedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);

            Assert.NotNull(callbackValue);
            Assert.Equal(questionResponse, callbackValue[questionId]);
        }

        [Fact]
        public async Task Submit_ShouldCallCacheProvider_ToGetFormData()
        {
            // Arrange
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123454");
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("testUrl")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            await _service.ProcessSubmission("form");

            // Assert
            _mockDistrubutedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Submit__Application_ShoudlThrowApplicationException_WhenGatewayResponse_IsNotOk()
        {
            // Arrange
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123454");
            var element = new ElementBuilder()
                 .WithType(EElementType.H1)
                 .WithQuestionId("test-id")
                 .WithPropertyText("test-text")
                 .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("test-url")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var cacheData = new FormAnswers
            {
                Path = "page-one"
            };

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessSubmission("form"));

            // Assert
            Assert.StartsWith("SubmitService::ProcessSubmission, An exception has occured while attemping to call ", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Submit_ShouldReturnModel_OnSuccessfulGatewayCall_And_DeleteCacheEntry()
        {
            // Arrange
            var guid = Guid.NewGuid();
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123454");

            var formData = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("testUrl")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(formData)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("\"1234456\"")
               });

            _sessionHelper.Setup(_ => _.GetSessionGuid())
                .Returns(guid.ToString());


            // Act
            var result = await _service.ProcessSubmission("form");

            // Assert
            var viewResult = Assert.IsType<SubmitServiceEntity>(result);

            _sessionHelper.Verify(_ => _.RemoveSessionGuid(), Times.Once);
            _mockDistrubutedCache.Verify(_ => _.Remove(It.Is<string>(x => x == guid.ToString())), Times.Once);
            Assert.Equal("Submit", viewResult.ViewName);
        }
    }
}
