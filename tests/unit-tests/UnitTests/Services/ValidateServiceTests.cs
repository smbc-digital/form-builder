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
using form_builder.Services.RetrieveExternalDataService.Entities;
using form_builder.Services.ValidateService;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class ValidateServiceTests
    {
        private readonly ValidateService _service;
        private readonly Mock<IGateway> _mockGateway = new();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCacheWrapper = new();
        private readonly Mock<IMappingService> _mockMappingService = new();
        private readonly Mock<IActionHelper> _mockActionHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        private readonly List<IAction> _pageActions = new()
        {
            new ActionBuilder()
                .WithActionType(EActionType.Validate)
                .WithPageActionSlug(new PageActionSlug
                {
                    URL = "www.test.com",
                    AuthToken = "authToken",
                    Environment = "local"
                })
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
                                new()
                                {
                                    Response = "testResponse",
                                    QuestionId = "testQuestionId"
                                }
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

        public ValidateServiceTests()
        {
            _service = new ValidateService(_mockGateway.Object, _mockSessionHelper.Object, _mockDistributedCacheWrapper.Object, _mockMappingService.Object, _mockActionHelper.Object, _mockHostingEnv.Object);

            _mockSessionHelper.Setup(_ => _.GetBrowserSessionId()).Returns("123456");
            _mockMappingService.Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<FormSchema>())).ReturnsAsync(_mappingEntity);
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(_successResponse);
            _mockActionHelper.Setup(_ => _.GenerateUrl(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns(new RequestEntity
                {
                    Url = "www.test.com / testResponse",
                    IsPost = false
                });
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
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
        public async Task Process_ShouldThrowApplicationException_IfResponseNotStatus200()
        {
            // Arrange
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = null
                });

            // Act & Assert
            ApplicationException result = await Assert.ThrowsAsync<ApplicationException>(() => _service.Process(_pageActions, null, "test"));
            Assert.Contains("ValidateService::Process, http request to www.test.com / testResponse returned an unsuccessful status code, Response:", result.Message);
        }
    }
}
