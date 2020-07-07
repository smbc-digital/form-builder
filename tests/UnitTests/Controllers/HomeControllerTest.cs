using System.Collections.Generic;
using form_builder.Controllers;
using Xunit;
using Moq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using form_builder.Enum;
using form_builder_tests.Builders;
using form_builder.Services.PageService;
using form_builder.Services.PageService.Entities;
using form_builder.Models;
using form_builder.Workflows;
using form_builder.Services.FileUploadService;
using form_builder.Builders;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Models.Properties.ElementProperties;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace form_builder_tests.UnitTests.Controllers
{
    public class HomeControllerTest
    {
        private HomeController _homeController;
        private readonly Mock<IPageService> _pageService = new Mock<IPageService>();
        private readonly Mock<ISubmitWorkflow> _submitWorkflow = new Mock<ISubmitWorkflow>();
        private readonly Mock<IPaymentWorkflow> _paymentWorkflow = new Mock<IPaymentWorkflow>();
        private readonly Mock<IFileUploadService> _mockFileUploadService = new Mock<IFileUploadService>();
        private readonly Mock<IHostingEnvironment> _mockHostingEnv = new Mock<IHostingEnvironment>();
        private readonly Mock<IActionsWorkflow> _mockActionsWorkflow = new Mock<IActionsWorkflow>();
        private readonly Mock<ISuccessWorkflow> _mockSucessWorkflow = new Mock<ISuccessWorkflow>();

        public HomeControllerTest()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _homeController = new HomeController(
                _pageService.Object,
                _submitWorkflow.Object,
                _paymentWorkflow.Object,
                _mockFileUploadService.Object,
                _mockHostingEnv.Object,
                _mockActionsWorkflow.Object,
                _mockSucessWorkflow.Object) {TempData = tempData};

            _mockSucessWorkflow.Setup(_ => _.Process(It.IsAny<string>())).ReturnsAsync(new SuccessPageEntity
            {
                FormAnswers = new FormAnswers(),
                FormName = "form",
                ViewName = "Success"
            });
        }

        [Fact]
        public void Home_ShouldRedirectTo_www_When_Prod_Env_ForHomeRoute()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("prod");
            var result = _homeController.Home();

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://www.stockport.gov.uk", redirectResult.Url);
        }

        [Fact]
        public void Home_ShouldReturnErrorView_WhenNonProdEnv_ForHomeRoute()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
            var result = _homeController.Home();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("../Error/Index", viewResult.ViewName);
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
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string,dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.GoToExternalPage, PageSlug = "https://www.bbc.co.uk/weather/2636882" });

            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel, null);

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
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();


            // Arrange
            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.GoToPage, PageSlug = "page-two" });

            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel, null);

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
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.SubmitForm });

            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel,null);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Submit", viewResult.ActionName);
        }

        [Fact]
        public async Task Index_ShouldThrowApplicationException_ShouldRunDefaultBehaviour()
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
                .WithValidatedModel(true)
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.Unknown });

            var viewModel = new Dictionary<string, string[]>();

            // Act & Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _homeController.Index("form", "page-one", viewModel,null));
            Assert.Equal($"The provided behaviour type 'Unknown' is not valid", result.Message);

        }

        [Fact]
        public async Task Index_Post_ShouldReturnView_WhenPageIsInvalid()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithAddressProvider("testAddressProvider")
               .WithQuestionId("test-address")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page, ViewName = "Search", UseGeneratedViewModel = true });

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await _homeController.Index("form", "page-one", viewModel, null, "automatic");
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Search", viewResult.ViewName);
        }

        [Fact]
        public async Task Index_Post_ShouldRedirectWhen_RedirectToAction_IsReturnedFrom_ProcessRequest()
        {
            var actionName = "AddressManual";

            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithAddressProvider("testAddressProvider")
               .WithQuestionId("test-address")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { RedirectAction = actionName, RedirectToAction = true });

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await _homeController.Index("testform", "page-one", viewModel, null, "automatic");
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(redirectResult.RouteValues.ContainsKey("form"));
            Assert.True(redirectResult.RouteValues.ContainsKey("path"));
            Assert.Contains("testform",redirectResult.RouteValues.Values);
            Assert.Contains("page-one",redirectResult.RouteValues.Values);
            Assert.Equal(actionName, redirectResult.ActionName);
        }

        [Theory]
        [InlineData("Submit", EBehaviourType.SubmitForm)]
        [InlineData("Index", EBehaviourType.GoToPage)]
        public async Task Index_Post_Should_PerformRedirectToAction_WhenPageIsValid_And_SelectJourney_OnBehaviour(string viewName, EBehaviourType behaviourType)
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address")
               .WithAddressProvider("testAddressProvider")
               .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(behaviourType)
                .WithPageSlug("url")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = behaviourType });

            var result = await _homeController.Index("form", "page-one", viewModel, null, "automatic");

            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(viewName, viewResult.ActionName);
        }

        [Fact]
        public async Task Index_Post_Should_PerformGoToExternalPageBehaviour_WhenPageIsValid_And_SelectJourney()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address")
               .WithAddressProvider("testAddressProvider")
               .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .WithPageSlug("submit-url")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.GoToExternalPage, PageSlug = "submit-url" });

            var result = await _homeController.Index("form", "page-one", viewModel, null, "automatic");

            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("submit-url", redirectResult.Url);
        }

        [Fact]
        public async Task Index_ShouldCallPageService()
        {
            //Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProcessPageEntity());

            //Act
            var result = await _homeController.Index("form", "path");


            //Assert
            _pageService.Verify(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Index_ShouldReturnViewResult()
        {
            //Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProcessPageEntity());

            //Act
            var result = await _homeController.Index("form", "path");

            //Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ShouldRedirect_WhenOnRedirectIsTrue()
        {
            //Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProcessPageEntity {  ShouldRedirect = true });

            //Act
            var result = await _homeController.Index("form", "path");

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Submit_ShouldRedirect_ToSuccessAction_OnSuccess()
        {
            // Arrange
            _submitWorkflow.Setup(_ => _.Submit(It.IsAny<string>())).ReturnsAsync(string.Empty);

            // Act
            var result = await _homeController.Submit("form");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _submitWorkflow.Verify(_ => _.Submit(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Index_ShouldRedirectToUrlWhen_SubmitAndPay()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Textbox)
               .WithQuestionId("test")
               .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithPageSlug("url")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry($"test", "test")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.SubmitAndPay });
            _paymentWorkflow.Setup(_ => _.Submit(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://www.return.url");

            var result = await _homeController.Index("form", "page-one", viewModel, null);

            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public async Task Index_ShouldAddFileUploadToViewModel_WhenSupplied()
        {
            var element = new ElementBuilder()
              .WithType(EElementType.FileUpload)
              .WithQuestionId("test")
              .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithPageSlug("url")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.SubmitAndPay });
            _paymentWorkflow.Setup(_ => _.Submit(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://www.return.url");

            var collection = new List<CustomFormFile>();
            var file = new CustomFormFile(String.Empty, "fileName", 0, "fileName");
            collection.Add(file);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .Build();

            await _homeController.Index("form", "page-one", viewModel, collection);

            _pageService.Verify(service => service.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), collection), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Index_ShouldCallActionsWorkflow_IfPageHasActions()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("test")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithPageSlug("url")
                .Build();

            var pageActions = new PageActionsBuilder()
                .WithActionType(EPageActionType.RetrieveExternalData)
                .WithActionProperties(new BaseActionProperty
                {
                    URL = string.Empty,
                    TargetQuestionId = string.Empty,
                    AuthToken = string.Empty
                })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .WithPageActions(pageActions)
                .Build();

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry($"test", "test")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.SubmitAndPay });
            _paymentWorkflow.Setup(_ => _.Submit(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://www.return.url");

            // Act
            await _homeController.Index("form", "page-one", viewModel, null);
            
            // Assert
            _mockActionsWorkflow.Verify(_ => _.Process(page, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Index_ShouldNotCallActionsWorkflow_IfPageHasNoActions()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("test")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithPageSlug("url")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry($"test", "test")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.SubmitAndPay });
            _paymentWorkflow.Setup(_ => _.Submit(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://www.return.url");

            // Act
            await _homeController.Index("form", "page-one", viewModel, null);

            // Assert
            _mockActionsWorkflow.Verify(_ => _.Process(page, It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Success_ShouldCallSuccessWorkflow()
        {
            // Act
            await _homeController.Success("form");

            // Assert
            _mockSucessWorkflow.Verify(_ => _.Process("form"), Times.Once);
        }

        [Fact]
        public async Task Success_ShouldReturnView()
        {
            // Act
            var result = await _homeController.Success("form");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Success", viewResult.ViewName);
        }
    }
}
