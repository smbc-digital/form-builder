using System.Dynamic;
using System.Net;
using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.RetrieveExternalDataService;
using form_builder.Services.RetrieveExternalDataService.Entities;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class RetrieveExternalDataServiceTests
    {
        private readonly RetrieveExternalDataService _service;
        private readonly Mock<IGateway> _mockGateway = new();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCacheWrapper = new();
        private readonly Mock<IMappingService> _mockMappingService = new();
        private readonly Mock<IActionHelper> _mockActionHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        private readonly List<IAction> _pageActions = new()
        {
            new ActionBuilder()
                .WithActionType(EActionType.RetrieveExternalData)
                .WithPageActionSlug(new PageActionSlug
                {
                    URL = "www.test.com",
                    AuthToken = "authToken",
                    Environment = "local"
                })
                .WithTargetQuestionId("targetId")
                .Build()
        };

        private readonly MappingEntity _mappingEntity =
            new MappingEntityBuilder()
                .WithFormAnswers(new FormAnswers
                {
                    Path = "page-one",
                    Pages = new List<PageAnswers>
                    {
                        new()
                        {
                            Answers = new List<Answers>
                            {
                                new("testResponse", "testQuestionId")
                            },
                            PageSlug = "page-one"
                        }
                    }
                })
                .WithData(new ExpandoObject())
                .Build();

        private readonly HttpResponseMessage _successResponse = new()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("\"test\"")
        };

        public RetrieveExternalDataServiceTests()
        {
            _service = new RetrieveExternalDataService(_mockGateway.Object, _mockSessionHelper.Object, _mockDistributedCacheWrapper.Object, _mockMappingService.Object, _mockActionHelper.Object, _mockHostingEnv.Object);

            _mockSessionHelper.Setup(_ => _.GetBrowserSessionId()).Returns("123456");
            _mockMappingService.Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<FormSchema>())).ReturnsAsync(_mappingEntity);
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(_successResponse);
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(_successResponse);
            _mockActionHelper.Setup(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns(new RequestEntity
                {
                    Url = "www.test.com/testResponse",
                    IsPost = false
                });
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task Process_ShouldNot_AddDataToAdditionalFormData_ByDefault()
        {
            // Arrange
            var action = new ActionBuilder()
               .WithActionType(EActionType.RetrieveExternalData)
               .WithPageActionSlug(new PageActionSlug
               {
                   URL = "www.test.com",
                   AuthToken = string.Empty,
                   Environment = "local"
               })
               .WithTargetQuestionId("targetId")
               .Build();

            var actions = new List<IAction> { action };

            // Act
            await _service.Process(actions, new FormSchema(), "test");

            // Assert
            Assert.Empty(_mappingEntity.FormAnswers.AdditionalFormData);
            Assert.False(_mappingEntity.FormAnswers.AdditionalFormData.ContainsKey("targetId"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("Real Data Response")]
        public async Task Process_Should_AddAnswerToFormPage_If_OKResponseIsValidStringNullOrEmpty(string response)
        {
            // Arrange
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = response is null ? null : new StringContent(System.Text.Json.JsonSerializer.Serialize(response))
                });

            string questionId = "testQuestion";

            List<IAction> actions = new()
            {
                new ActionBuilder()
                   .WithActionType(EActionType.RetrieveExternalData)
                   .WithPageActionSlug(new PageActionSlug
                   {
                       URL = "www.test.com",
                       AuthToken = string.Empty,
                       Environment = "local"
                   })
                   .WithTargetQuestionId(questionId)
                   .Build()
            };

            // Act
            await _service.Process(actions, new FormSchema(), "test");

            // Assert
            var page = _mappingEntity.FormAnswers.Pages.First(page => page.PageSlug.Equals("page-one"));
            var answer = page.Answers.First(answer => answer.QuestionId.Equals(questionId));

            Assert.True(answer is not null);
        }

        [Fact]
        public async Task Process_Should_SetEmptyAnswerToFormPage_If_ResponseIsNotFound()
        {
            // Arrange
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = null
                });

            string questionId = "testQuestion";

            List<IAction> actions = new()
            {
                new ActionBuilder()
                   .WithActionType(EActionType.RetrieveExternalData)
                   .WithPageActionSlug(new PageActionSlug
                   {
                       URL = "www.test.com",
                       AuthToken = string.Empty,
                       Environment = "local"
                   })
                   .WithTargetQuestionId(questionId)
                   .Build()
            };

            // Act
            await _service.Process(actions, new FormSchema(), "test");

            // Assert
            var page = _mappingEntity.FormAnswers.Pages.First(page => page.PageSlug.Equals("page-one"));
            var answer = page.Answers.First(answer => answer.QuestionId.Equals(questionId));

            Assert.True(answer.Response is null);
        }

        [Fact]
        public async Task Process_Should_AddDataToAdditionalFormData_If_IncludeInFormSubmission()
        {
            // Arrange
            var action = new ActionBuilder()
               .WithActionType(EActionType.RetrieveExternalData)
               .WithPageActionSlug(new PageActionSlug
               {
                   URL = "www.test.com",
                   AuthToken = string.Empty,
                   Environment = "local"
               })
               .WithTargetQuestionId("targetId")
               .Build();

            action.Properties.IncludeInFormSubmission = true;
            var actions = new List<IAction> { action };

            // Act
            await _service.Process(actions, new FormSchema(), "test");

            // Assert
            Assert.Single(_mappingEntity.FormAnswers.AdditionalFormData);
            Assert.True(_mappingEntity.FormAnswers.AdditionalFormData.ContainsKey("targetId"));
        }

        [Fact]
        public async Task Process_Should_NotCallGatewayToUpdateHeader_IfNoAuthTokenProvided()
        {
            // Arrange
            var action = new ActionBuilder()
               .WithActionType(EActionType.RetrieveExternalData)
               .WithPageActionSlug(new PageActionSlug
               {
                   URL = "www.test.com",
                   AuthToken = string.Empty,
                   Environment = "local"
               })
               .WithTargetQuestionId("targetId")
               .Build();

            var actions = new List<IAction>
            {
                action
            };

            // Act
            await _service.Process(actions, new FormSchema(), "test");

            // Assert
            _mockGateway.Verify(_ => _.ChangeAuthenticationHeader(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Process_ShouldCallGatewayToUpdateHeader_IfAuthTokenProvided()
        {
            // Act
            await _service.Process(_pageActions, new FormSchema(), "test");

            // Assert
            _mockGateway.Verify(_ => _.ChangeAuthenticationHeader(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldCallGateway_PostAsync()
        {
            // Arrange
            _mockActionHelper.Setup(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns(new RequestEntity
                {
                    Url = string.Empty,
                    IsPost = true
                });

            // Act
            await _service.Process(_pageActions, new FormSchema(), "test");

            // Assert
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldCallGateway_GetAsync()
        {
            // Arrange
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(_successResponse);

            // Act
            await _service.Process(_pageActions, new FormSchema(), "test");

            // Assert
            _mockGateway.Verify(_ => _.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldThrowApplicationException_IfNoSuccessStatusCode()
        {
            // Arrange
            var actions = new List<IAction>
            {
                new ActionBuilder()
                    .WithPageActionSlug(new PageActionSlug
                    {
                        URL = "www.test.com/{{testQuestionId}}",
                        AuthToken = string.Empty,
                        Environment = "local"
                    })
                    .WithTargetQuestionId("targetId")
                    .Build()
            };

            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway
                });

            // Act & Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.Process(actions, null, "test"));
            Assert.Contains("RetrieveExternalDataService::Process, http request to www.test.com/testResponse returned an unsuccessful status code, Response: ", result.Message);
        }

        [Fact]
        public async Task Process_ShouldCallDistributedCache_SetStringAsync()
        {
            // Arrange
            _mockActionHelper.Setup(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns(new RequestEntity
                {
                    Url = string.Empty,
                    IsPost = false
                });

            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(_successResponse);

            // Act
            await _service.Process(_pageActions, new FormSchema(), "test");

            // Assert
            _mockDistributedCacheWrapper.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
