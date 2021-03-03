using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.SubmitService;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class SubmitServiceTests
    {
        private readonly SubmitService _service;
        private readonly Mock<IGateway> _mockGateway = new Mock<IGateway>();
        private readonly Mock<IPageHelper> _mockPageHelper = new Mock<IPageHelper>();
        private readonly Mock<IWebHostEnvironment> _mockEnvironment = new Mock<IWebHostEnvironment>();
        private readonly Mock<IOptions<SubmissionServiceConfiguration>> _mockIOptions = new Mock<IOptions<SubmissionServiceConfiguration>>();
        public SubmitServiceTests()
        {
            _mockEnvironment.Setup(_ => _.EnvironmentName)
                .Returns("local");

            _mockIOptions.Setup(_ => _.Value)
                .Returns(new SubmissionServiceConfiguration
                {
                    FakePaymentSubmission = false
                });

            _service = new SubmitService(_mockGateway.Object, _mockPageHelper.Object, _mockEnvironment.Object, _mockIOptions.Object);
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
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            // Act
            var result = await Assert.ThrowsAsync<NullReferenceException>(() => _service.ProcessSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.Equal("Page model::GetSubmitFormEndpoint, No postUrl supplied for submit form", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task ProcessSubmission_Applicaton_ShouldCatchException_WhenGatewayCallThrowsException()
        {
            // Arrange
            var submitSlug = new SubmitSlug() { AuthToken = "AuthToken", Environment = "local", URL = "www.Environment.com" };

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
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _service.PaymentSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            Assert.StartsWith("error", result.Message);
        }

        [Fact]
        public async Task ProcessSubmission_ShouldCallGateway_WithFormData()
        {
            // Arrange
            var questionId = "testQuestion";
            var callbackValue = new ExpandoObject() as IDictionary<string, object>;

            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .Build();

            var submitSlug = new SubmitSlug() { AuthToken = "AuthToken", Environment = "local", URL = "www.location.com" };

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

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                })
                .Callback<string, object>((x, y) => callbackValue = (ExpandoObject)y);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

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

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

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

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("\"1234456\"")
               });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
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

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

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

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = null
               });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
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

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("")
               });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.PaymentSubmission(new MappingEntity { BaseForm = schema, FormAnswers = new FormAnswers { Path = "page-one" } }, "form", ""));

            // Assert
            Assert.StartsWith($"SubmitService::PaymentSubmission, Gateway", result.Message);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

    }
}
