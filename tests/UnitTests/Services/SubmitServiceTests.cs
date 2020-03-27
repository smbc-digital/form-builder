using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
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
using Xunit;
using System.Dynamic;
using form_builder.Services.MappingService.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using form_builder.ViewModels;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using Microsoft.Extensions.Options;
using form_builder.Configuration;
using form_builder.Builders;

namespace form_builder_tests.UnitTests.Services
{
    public class SubmitServiceTests
    {
        private readonly SubmitService _service;
        private readonly Mock<ILogger<SubmitService>> _mockLogger = new Mock<ILogger<SubmitService>>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistrubutedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IGateway> _mockGateway = new Mock<IGateway>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<ISessionHelper> _sessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IHostingEnvironment> _mockEnvironment = new Mock<IHostingEnvironment>();
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistrbutedCacheExpirationConfiguration = new Mock<IOptions<DistributedCacheExpirationConfiguration>>();
        public SubmitServiceTests()
        {
            _mockEnvironment.Setup(_ => _.EnvironmentName)
                .Returns("local");

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

            _mockDistrbutedCacheExpirationConfiguration.Setup(_ => _.Value).Returns(new DistributedCacheExpirationConfiguration
            {
                Document = 1
            });

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(Newtonsoft.Json.JsonConvert.SerializeObject(cacheData));

            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Host)
                .Returns(new HostString("www.test.com"));

            _service = new SubmitService(_mockLogger.Object, _mockDistrubutedCache.Object, _mockGateway.Object, _pageHelper.Object, _sessionHelper.Object, _mockEnvironment.Object, _mockHttpContextAccessor.Object, _mockDistrbutedCacheExpirationConfiguration.Object);
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

            // Act
            var result = await Assert.ThrowsAsync<NullReferenceException>(() => _service.ProcessSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

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
            SubmitSlug submitSlug = new SubmitSlug() { AuthToken = "AuthToken", Environment = "local", URL = "www.Environment.com" };

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

            // Act & Assert
            //var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));
            var result = await Assert.ThrowsAsync<Exception>(() => _service.PaymentSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            Assert.StartsWith("error", result.Message);
        }

        [Fact]
        public async Task ProcessSubmission_ShouldCallGateway_WithFormData()
        {
            // Arrange
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

            SubmitSlug submitSlug = new SubmitSlug() { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

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

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                })
                .Callback<string, object>((x, y) => callbackValue = (ExpandoObject)y);
            // Act
            await _service.ProcessSubmission(new MappingEntity { Data = new ExpandoObject(), BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", "123454");

            // Assert
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);

            Assert.NotNull(callbackValue);
        }

        [Fact]
        public async Task ProcessSubmission__Application_ShoudlThrowApplicationException_WhenGatewayResponse_IsNotOk()
        {
            // Arrange
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123454");
            var element = new ElementBuilder()
                 .WithType(EElementType.H1)
                 .WithQuestionId("test-id")
                 .WithPropertyText("test-text")
                 .Build();

            var submitSlug = new SubmitSlug() { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

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
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith("SubmitService::ProcessSubmission, An exception has occurred while attempting to call ", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PaymentSubmission_ShouldCallGateway_AndReturn_Reference()
        {
            // Arrange
            var guid = Guid.NewGuid();

            SubmitSlug submitSlug = new SubmitSlug() { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

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
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("\"1234456\"")
               });

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
            var guid = Guid.NewGuid();
            SubmitSlug submitSlug = new SubmitSlug() { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

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

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.PaymentSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith("SubmitService::PaymentSubmission, An exception has occured while attempting to call ", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PaymentSubmission_ShouldThrowApplicationException_WhenNoContentFromGateway()
        {
            // Arrange
            var guid = Guid.NewGuid();
            SubmitSlug submitSlug = new SubmitSlug() { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };
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
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = null
               });

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.PaymentSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith("SubmitService::PaymentSubmission, An exception has occured when response content from ", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PaymentSubmission_ShouldThrowApplicationException_WhenGatewayResponseContent_IsEmpty()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var postUrl = "www.post.url";
            SubmitSlug submitSlug = new SubmitSlug() { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

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

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("")
               });

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.PaymentSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith($"SubmitService::PaymentSubmission, Gateway", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

    }
}
