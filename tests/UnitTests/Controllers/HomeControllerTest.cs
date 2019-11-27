using System.Collections.Generic;
using form_builder.Controllers;
using form_builder.Validators;
using Xunit;
using Moq;
using StockportGovUK.AspNetCore.Gateways;
using System.Threading.Tasks;
using System;
using form_builder.Models;
using Microsoft.AspNetCore.Mvc;
using form_builder.ViewModels;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using Newtonsoft.Json;
using form_builder_tests.Builders;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using StockportGovUK.NetStandard.Models.Addresses;
using form_builder.Helpers.Session;

namespace form_builder_tests.UnitTests.Controllers
{
    public class HomeControllerTest
    {
        private HomeController _homeController;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private readonly Mock<ISchemaProvider> _schemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IGateway> _gateWay = new Mock<IGateway>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<ILogger<HomeController>> _logger = new Mock<ILogger<HomeController>>();
        private readonly Mock<ISessionHelper> _mockSession = new Mock<ISessionHelper>();

        public HomeControllerTest()
        {
            _mockDistributedCache = new Mock<IDistributedCacheWrapper>();

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()))
                        .ReturnsAsync(new FormBuilderViewModel());

            var cacheData = new FormAnswers
            {
                Path = "page-one"
            };

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            _mockSession.Setup(_ => _.GetSessionGuid()).Returns(Guid.NewGuid().ToString);

            _homeController = new HomeController(_logger.Object, _mockDistributedCache.Object, _validators.Object, _schemaProvider.Object, _gateWay.Object, _pageHelper.Object, _mockSession.Object);
        }

        [Fact]
        public async Task Index_ShouldCallSchemaProvider_ToGetFormSchema()
        {
            // Act
            var result = await _homeController.Index("form", "page-one");

            // Assert
            _schemaProvider.Verify(_ => _.Get<FormSchema>(It.Is<string>(x => x == "form")));
        }


        [Fact]
        public async Task Index_ShouldRedirectToAddressController_WhenTypeContainsAddress()
        {
            var element = new ElementBuilder()
                 .WithType(EElementType.Address)
                 .WithQuestionId("address-test")
                 .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            var result = await _homeController.Index("form", "page-one");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Address", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_ShouldGenerateGuidWhenGuidIsEmpty()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString();
            _mockSession.Setup(_ => _.GetSessionGuid()).Returns(string.Empty);

            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            var result = await _homeController.Index("form", "page-one");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = (FormBuilderViewModel)viewResult.Model;

            _mockSession.Verify(_ => _.GetSessionGuid(), Times.Once);
            _mockSession.Verify(_ => _.SetSessionGuid(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Index_ShouldRedirectToError_WhenPageIsNotWithin_FormSchema()
        {
            // Arrange
            var element = new ElementBuilder()
              .WithType(EElementType.H1)
              .WithQuestionId("test-id")
              .WithPropertyText("test-text")
              .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            var result = await _homeController.Index("form", "non-existance-page");

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", viewResult.ActionName);
        }

        [Fact]
        public async Task Index_ShouldRunBehaviourForRedirect_GoToExternalPage()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .WithPageSlug("https://www.bbc.co.uk/weather/2636882")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel);

            // Assert
            var viewResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://www.bbc.co.uk/weather/2636882", viewResult.Url);
        }

        [Fact]
        public async Task Index_ShouldRunBehaviourForRedirect_GoToPage()
        {
            var element = new ElementBuilder()
              .WithType(EElementType.H1)
              .WithQuestionId("test-id")
              .WithPropertyText("test-text")
              .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-two")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Arrange
            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
               .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(viewResult.RouteValues.ContainsKey("path"));
            Assert.True(viewResult.RouteValues.Values.Contains("page-two"));
            Assert.Equal("Index", viewResult.ActionName);
        }

        [Fact]
        public async Task Index_ShouldRunBehaviourForRedirectToAction_SubmitForm()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
               .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Submit", viewResult.ActionName);
        }

        [Fact]
        public async Task Index_ShouldRunDefaultBehaviour()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.Unknown)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
               .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", viewResult.ActionName);
        }

        [Fact]
        public async Task Submit_ShouldCallCacheProvider_ToGetFormData()
        {
            // Arrange
            _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new HttpResponseMessage
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

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            await _homeController.Submit("form");

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Submit_ShouldCallGateway_WithFormData()
        {
            // Arrange
            var questionId = "test-question";
            var questionResponse = "test-response";
            var callbackValue = new PostData();
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

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));
            _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                })
                .Callback<string, object>((x, y) => callbackValue = (PostData)y);
            // Act
            await _homeController.Submit("form");

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            _gateWay.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);

            Assert.NotNull(callbackValue);
            Assert.Equal(questionId, callbackValue.Answers[0].QuestionId);
            Assert.Equal(questionResponse, callbackValue.Answers[0].Response);
        }

        [Fact]
        public async Task Submit_ShouldReturnErrorView_WhenGatewayCallFails()
        {
            // Arrange
            var guid = Guid.NewGuid();

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

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("error"));

            // Act & Assert
            var result = await _homeController.Submit("form");

            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", viewResult.ActionName);
        }

        [Fact]
        public async Task Submit_ShouldReturnView_OnSuccessfulGatewayCall_And_DeleteCacheEntry()
        {
            // Arrange
            var guid = Guid.NewGuid();

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

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<PostData>()))
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("\"1234456\"")
               });

            _mockSession.Setup(_ => _.GetSessionGuid())
                .Returns(guid.ToString());


                // Act
                var result = await _homeController.Submit("form");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            _mockSession.Verify(_ => _.RemoveSessionGuid(), Times.Once);
            _mockDistributedCache.Verify(_ => _.Remove(It.Is<string>(x => x == guid.ToString())), Times.Once);
            Assert.Equal("Submit", viewResult.ViewName);
        }

        [Fact]
        public async Task Submit_ShouldReturnErrorView_WhenGuid_IsEmpty()
        {
            //Arrange 
            var guid = string.Empty;
            _mockSession.Setup(_ => _.GetSessionGuid())
                .Returns(guid);

            // Act
            var result = await _homeController.Submit("form");

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", viewResult.ActionName);

        }

        [Fact]
        public async Task Submit_ShouldRedirectToError_WhenNoSubmitUrlSpecified()
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

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            var result = await _homeController.Submit("form");

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", viewResult.ActionName);
            _gateWay.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task Submit_ShouldRedirectToError_WhenGatewayResponse_IsNotOk()
        {
            // Arrange
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

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var cacheData = new FormAnswers
            {
                Path = "page-one"
            };

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new System.Net.Http.HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

            // Act
            var result = await _homeController.Submit("form");

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", viewResult.ActionName);
            _gateWay.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
