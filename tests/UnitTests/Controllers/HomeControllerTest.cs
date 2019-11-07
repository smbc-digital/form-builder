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

        public HomeControllerTest()
        {
            _mockDistributedCache = new Mock<IDistributedCacheWrapper>();

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>()))
             .ReturnsAsync(new FormBuilderViewModel());

            _homeController = new HomeController(_logger.Object, _mockDistributedCache.Object, _validators.Object, _schemaProvider.Object, _gateWay.Object, _pageHelper.Object);
        }

        [Fact]
        public async Task Index_ShouldCallSchemaProvider_ToGetFormSchema()
        {
            var result = await _homeController.Index("form", "page-one", Guid.NewGuid());

            _schemaProvider.Verify(_ => _.Get<FormSchema>(It.Is<string>(x => x == "form")));
        }

        [Fact]
        public async Task Index_ShouldGenerateGuidWhenGuidIsEmpty()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageUrl("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var result = await _homeController.Index("form", "page-one", Guid.Empty);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = (FormBuilderViewModel)viewResult.Model;

            Assert.NotEqual(Guid.Empty, model.Guid);
        }

        [Fact]
        public async Task Index_ShouldRedirectToError_WhenPageIsNotWithin_FormSchema()
        {
            var element = new ElementBuilder()
              .WithType(EElementType.H1)
              .WithQuestionId("test-id")
              .WithPropertyText("test-text")
              .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageUrl("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var result = await _homeController.Index("form", "non-existance-page", Guid.Empty);

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
                .WithPageUrl("https://www.bbc.co.uk/weather/2636882")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageUrl("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("Guid", Guid.NewGuid().ToString());

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
                .WithPageUrl("page-two")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageUrl("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Arrange
            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
               .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, string>();
            var guid = Guid.NewGuid();
            viewModel.Add("Guid", guid.ToString());

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(viewResult.RouteValues.ContainsKey("path"));
            Assert.True(viewResult.RouteValues.ContainsKey("guid"));
            Assert.True(viewResult.RouteValues.Values.Contains("page-two"));
            Assert.True(viewResult.RouteValues.Values.Contains(guid));
            Assert.Equal("Index", viewResult.ActionName);
            Assert.Equal("Home", viewResult.ControllerName);
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
                .WithPageUrl("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
               .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, string>();
            var guid = Guid.NewGuid();
            viewModel.Add("Guid", guid.ToString());

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(viewResult.RouteValues.ContainsKey("guid"));
            Assert.True(viewResult.RouteValues.Values.Contains(guid));
            Assert.Equal("Submit", viewResult.ActionName);
            Assert.Equal("Home", viewResult.ControllerName);
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
                .WithPageUrl("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
               .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, string>();
            var guid = Guid.NewGuid();
            viewModel.Add("Guid", guid.ToString());

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
            var guid = Guid.NewGuid();
            var cacheData = new List<FormAnswers>();
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(cacheData));

            // Act
            await _homeController.Submit(guid);

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.Is<string>(x => x == guid.ToString())), Times.Once);
        }

        [Fact]
        public async Task Submit_ShouldCallGateway_WithFormData()
        {
            // Arrange
            var questionId = "test-question";
            var questionResponse = "test-response";
            var callbackValue = new List<FormAnswers>();
            var guid = Guid.NewGuid();
            var cacheData = new List<FormAnswers>
            {
                new FormAnswers
                {
                    PageUrl = "page-one",
                    Answers = new List<Answers>
                    {
                        new Answers
                        {
                             QuestionId = questionId,
                             Response = questionResponse
                        }
                    }
                }
            };

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));
            _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((x, y) => callbackValue = (List<FormAnswers>)y);

            // Act
            await _homeController.Submit(guid);

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.Is<string>(x => x == guid.ToString())), Times.Once);
            _gateWay.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);

            Assert.NotNull(callbackValue);
            Assert.Equal(questionId, callbackValue[0].Answers[0].QuestionId);
            Assert.Equal(questionResponse, callbackValue[0].Answers[0].Response);
        }

        [Fact]
        public async Task Submit_ShouldThrowException_WhenGatewayCallFails()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var cacheData = new List<FormAnswers>();

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));
            _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => await _homeController.Submit(guid));
        }

        [Fact]
        public async Task Submit_ShouldReturnView_OnSuccessfulGatewayCall_And_DeleteCacheEntry()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var cacheData = new List<FormAnswers>();

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            // Act
            var result = await _homeController.Submit(guid);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            _mockDistributedCache.Verify(_ => _.Remove(It.Is<string>(x => x == guid.ToString())), Times.Once);
            Assert.Equal("Submit", viewResult.ViewName);
        }

        [Fact]
        public async Task Submit_ShouldReturnErrorView_WhenGuid_IsEmpty()
        {
            // Act
            var result = await _homeController.Submit(Guid.Empty);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", viewResult.ActionName);
            Assert.Equal("Home", viewResult.ControllerName);

        }
    }
}
