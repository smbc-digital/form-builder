using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Properties;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.RetrieveExternalDataService;
using form_builder_tests.Builders;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class RetrieveExternalDataServiceTests
    {
        private readonly RetrieveExternalDataService _service;
        private readonly Mock<IGateway> _mockGateway = new Mock<IGateway>();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCacheWrapper = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IMappingService> _mockMappingService = new Mock<IMappingService>();
        
        private readonly List<PageAction> pageActions = new List<PageAction>
        {
            new PageActionsBuilder()
                .WithProperties(new BaseProperty
                {
                    URL = "www.test.com",
                    TargetQuestionId = "targetId",
                    AuthToken = ""
                })
                .Build()
        };

        private readonly MappingEntity mappingEntity =
            new MappingEntityBuilder()
                .WithFormAnswers(new FormAnswers
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

        private readonly HttpResponseMessage successResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("\"test\"")
        }; 
            

        public RetrieveExternalDataServiceTests()
        {
            _service = new RetrieveExternalDataService(_mockGateway.Object, _mockSessionHelper.Object, _mockDistributedCacheWrapper.Object, _mockMappingService.Object);

            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123456");
            _mockMappingService.Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mappingEntity);
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(successResponse);
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(successResponse);
        }

        [Fact]
        public async Task Process_Should_CallGateway_IfNoAuthTokenProvided()
        {
            // Act
            await _service.Process(pageActions, "test");

            // Assert
            _mockGateway.Verify(_ => _.ChangeAuthenticationHeader(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Process_ShouldCallGateway_IfAuthTokenProvided()
        {
            // Arrange
            var actions = new List<PageAction>
            {
                new PageActionsBuilder()
                    .WithProperties(new BaseProperty
                    {
                        URL = "www.test.com",
                        TargetQuestionId = "targetId",
                        AuthToken = "authToken"
                    })
                    .Build()
            };

            // Act
            await _service.Process(actions, "test");

            // Assert
            _mockGateway.Verify(_ => _.ChangeAuthenticationHeader(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldCallGateway_PostAsync()
        {
            // Act
            await _service.Process(pageActions, "test");

            // Assert
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldCallGateway_GetAsync()
        {
            // Arrange
            var actions = new List<PageAction>
            {
                new PageActionsBuilder()
                    .WithProperties(new BaseProperty
                    {
                        URL = "www.test.com/{{testQuestionId}}",
                        TargetQuestionId = "targetId"
                    })
                    .Build()
            };

            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(successResponse);

            // Act
            await _service.Process(actions, "test");

            // Assert
            _mockGateway.Verify(_ => _.GetAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldThrowApplicationException_IfNoSuccessStatusCode()
        {
            // Arrange
            var actions = new List<PageAction>
            {
                new PageActionsBuilder()
                    .WithProperties(new BaseProperty
                    {
                        URL = "www.test.com/{{testQuestionId}}",
                        TargetQuestionId = "targetId",
                        AuthToken = ""
                    })
                    .Build()
            };

            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                     StatusCode = HttpStatusCode.BadGateway
                });

            // Act & Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.Process(actions, "test"));
            Assert.Contains($"RetrieveExternalDataService::Process, http request to www.test.com/testResponse returned an unsuccessful status code, Response: ", result.Message);
        }
    }
}
