using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Controllers;
using form_builder.Enum;
using form_builder.Helpers.Session;
using form_builder.Mappers.Structure;
using form_builder.Models;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Services.FileUploadService;
using form_builder.Services.PageService;
using form_builder.Services.PageService.Entities;
using form_builder.Workflows.ActionsWorkflow;
using form_builder.Workflows.EmailWorkflow;
using form_builder.Workflows.PaymentWorkflow;
using form_builder.Workflows.RedirectWorkflow;
using form_builder.Workflows.SubmitWorkflow;
using form_builder.Workflows.SuccessWorkflow;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Controllers
{
    public class HomeControllerTest
    {
        private readonly HomeController _homeController;
        private readonly Mock<IPageService> _pageService = new();
        private readonly Mock<ISubmitWorkflow> _submitWorkflow = new();
        private readonly Mock<IEmailWorkflow> _emailWorkflow = new();
        private readonly Mock<IPaymentWorkflow> _paymentWorkflow = new();
        private readonly Mock<IRedirectWorkflow> _redirectWorkflow = new();
        private readonly Mock<IFileUploadService> _mockFileUploadService = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();
        private readonly Mock<IActionsWorkflow> _mockActionsWorkflow = new();
        private readonly Mock<ISuccessWorkflow> _mockSuccessWorkflow = new();
        private readonly Mock<IStructureMapper> _mockStructureMapper = new();
        private readonly Mock<IOptions<DataStructureConfiguration>> _mockDataStructureConfiguration = new();
        private readonly Mock<ILogger<HomeController>> _mockLogger = new();

        private readonly Mock<ISessionHelper> _mockSessionHelper = new();

        public HomeControllerTest()
        {
            

            
            Mock<ISession> mockSession = new();
            mockSession.Setup(_ => _.IsAvailable).Returns(true);
            mockSession.Setup(_ => _.Id).Returns("SessionMockId");

            _mockSessionHelper.Setup(_ => _.GetBrowserSessionId())
                .Returns("d96bceca-f5c6-49f8-98ff-2d823090c198");

            _mockSessionHelper.Setup(_ => _.GetSession())
                .Returns(mockSession.Object);

            var context = new Mock<HttpContext>();
            context.SetupGet(_ => _.Request.Query)
                .Returns(new QueryCollection());

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _mockDataStructureConfiguration
                .Setup(_ => _.Value)
                .Returns(new DataStructureConfiguration { IsEnabled = true });

            _mockStructureMapper
                .Setup(_ => _.CreateBaseFormDataStructure(It.IsAny<string>()))
                .ReturnsAsync(new Dictionary<string, dynamic>());

            _homeController = new HomeController(
                _pageService.Object,
                _submitWorkflow.Object,
                _paymentWorkflow.Object,
                _redirectWorkflow.Object,
                _mockFileUploadService.Object,
                _mockHostingEnv.Object,
                _mockActionsWorkflow.Object,
                _emailWorkflow.Object,
                _mockSuccessWorkflow.Object,
                _mockStructureMapper.Object,
                _mockDataStructureConfiguration.Object,
                _mockLogger.Object,
                _mockSessionHelper.Object)
            { TempData = tempData };

            _homeController.ControllerContext = new ControllerContext();
            _homeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _homeController.ControllerContext.HttpContext.Request.Query = new QueryCollection();

            _mockSuccessWorkflow.Setup(_ => _.Process(It.IsAny<EBehaviourType>(), It.IsAny<string>())).ReturnsAsync(new SuccessPageEntity
            {
                FormAnswers = new FormAnswers(),
                FormName = "form",
                ViewName = "Success"
            });
        }

        [Fact]
        public void Home_ShouldRedirectTo_www_When_Prod_Env_ForHomeRoute()
        {
            // Arrange
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("prod");

            // Act
            var result = _homeController.Home();

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://www.stockport.gov.uk", redirectResult.Url);
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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.GoToExternalPage, PageSlug = "https://www.bbc.co.uk/weather/2636882" });

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
            // Arrange
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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.GoToPage, PageSlug = "page-two" });

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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.SubmitForm });

            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel, null);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Submit", viewResult.ActionName);
        }

        [Fact]
        public async Task Index_ShouldRunBehaviourForRedirectToAction_SubmitWithoutSubmission()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitWithoutSubmission)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.SubmitWithoutSubmission });

            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel, null);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SubmitWithoutSubmission", viewResult.ActionName);
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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.Unknown });

            var viewModel = new Dictionary<string, string[]>();

            // Act & Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _homeController.Index("form", "page-one", viewModel, null));
            Assert.Contains("The provided behaviour type 'Unknown' is not valid", result.Message);

        }

        [Fact]
        public async Task Index_Post_ShouldReturnView_WhenPageIsInvalid()
        {
            // Arrange
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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page, ViewName = "Search", UseGeneratedViewModel = true });

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel, null, "automatic");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Search", viewResult.ViewName);
        }

        [Fact]
        public async Task Index_Post_ShouldRedirectWhen_RedirectToAction_IsReturnedFrom_ProcessRequest()
        {
            // Arrange
            var actionName = "AddressManual";

            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithAddressProvider("testAddressProvider")
               .WithQuestionId("test-address")
               .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { RedirectAction = actionName, RedirectToAction = true });

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            // Act
            var result = await _homeController.Index("testform", "page-one", viewModel, null, "automatic");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(redirectResult.RouteValues.ContainsKey("form"));
            Assert.True(redirectResult.RouteValues.ContainsKey("path"));
            Assert.Contains("testform", redirectResult.RouteValues.Values);
            Assert.Contains("page-one", redirectResult.RouteValues.Values);
            Assert.Equal(actionName, redirectResult.ActionName);
        }

        [Theory]
        [InlineData("Submit", EBehaviourType.SubmitForm)]
        [InlineData("Index", EBehaviourType.GoToPage)]
        public async Task Index_Post_Should_PerformRedirectToAction_WhenPageIsValid_And_SelectJourney_OnBehaviour(string viewName, EBehaviourType behaviourType)
        {
            // Arrange
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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = behaviourType });

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel, null, "automatic");

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(viewName, viewResult.ActionName);
        }

        [Fact]
        public async Task Index_Post_Should_PerformGoToExternalPageBehaviour_WhenPageIsValid_And_SelectJourney()
        {
            // Arrange
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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.GoToExternalPage, PageSlug = "submit-url" });

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel, null, "automatic");

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("submit-url", redirectResult.Url);
        }

        [Fact]
        public async Task Index_ShouldCallPageService()
        {
            // Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IQueryCollection>()))
                .ReturnsAsync(new ProcessPageEntity());

            // Act
            await _homeController.Index("form", "path");


            // Assert
            _pageService.Verify(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IQueryCollection>()), Times.Once);
        }

        [Fact]
        public async Task Index_ShouldReturnViewResult()
        {
            // Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IQueryCollection>()))
                .ReturnsAsync(new ProcessPageEntity());

            // Act
            var result = await _homeController.Index("form", "path");

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ShouldRedirect_WhenOnRedirectIsTrue()
        {
            // Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IQueryCollection>()))
                .ReturnsAsync(new ProcessPageEntity { ShouldRedirect = true });

            var result = await _homeController.Index("form", "path");

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Index_ShouldRedirect_WhenOnRedirectIsTrue_WithQueryValues()
        {
            // Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IQueryCollection>()))
                .ReturnsAsync(new ProcessPageEntity { ShouldRedirect = true });

            // Act
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
            {
                {"test", new StringValues("value")},
                {"testtwo", new StringValues("testtwo")}
            });
            _homeController.ControllerContext = new ControllerContext();
            _homeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _homeController.ControllerContext.HttpContext.Request.Query = queryCollection;

            // _homeController.
            var result = await _homeController.Index("form", "path");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(redirectResult.RouteValues.ContainsKey("test"));
            Assert.True(redirectResult.RouteValues.ContainsKey("testtwo"));
            Assert.Equal(4, redirectResult.RouteValues.Count);
        }

        [Fact]
        public async Task Index_ShouldRedirect_WhenOnRedirectIsTrue_WithNoAdditionalRouteValues_WhenNoQueryParametersPassed()
        {
            // Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IQueryCollection>()))
                .ReturnsAsync(new ProcessPageEntity { ShouldRedirect = true });

            var result = await _homeController.Index("form", "path");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(2, redirectResult.RouteValues.Count);
        }


        [Fact]
        public async Task Submit_ShouldRedirect_ToSuccessAction_OnSuccess()
        {
            // Arrange
            _submitWorkflow.Setup(_ => _.Submit(It.IsAny<string>())).ReturnsAsync(string.Empty);

            // Act
            var result = await _homeController.Submit("form");

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            _submitWorkflow.Verify(_ => _.Submit(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Index_ShouldRedirectToUrlWhen_SubmitAndPay()
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
                .WithEntry("test", "test")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.SubmitAndPay });
            _paymentWorkflow.Setup(_ => _.Submit(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://www.return.url");

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel, null);

            // Assert
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public async Task Index_ShouldRedirectToUrlWhen_SubmitAndRedirect()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Textbox)
               .WithQuestionId("test")
               .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndRedirect)
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
                .WithEntry("test", "test")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.SubmitAndRedirect });
            _redirectWorkflow.Setup(_ => _.Submit(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://www.return.url");
            // Act
            var result = await _homeController.Index("form", "page-one", viewModel, null);

            // Assert
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public async Task Index_ShouldAddFileUploadToViewModel_WhenSupplied()
        {
            // Arrange
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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.SubmitAndPay });
            _paymentWorkflow.Setup(_ => _.Submit(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://www.return.url");

            var collection = new List<CustomFormFile>();
            var file = new CustomFormFile(String.Empty, "fileName", 0, "fileName");
            collection.Add(file);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .Build();

            // Act
            await _homeController.Index("form", "page-one", viewModel, collection);

            // Assert
            _pageService.Verify(service => service.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), collection, It.IsAny<bool>()), Times.AtLeastOnce);
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

            var pageActions = new ActionBuilder()
                .WithActionType(EActionType.RetrieveExternalData)
                .WithPageActionSlug(new PageActionSlug
                {
                    URL = "www.test.com",
                    Environment = "local",
                    AuthToken = string.Empty
                })
                .WithTargetQuestionId(string.Empty)
                .WithHttpActionType(EHttpActionType.Post)
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
                .WithEntry("test", "test")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.SubmitAndPay });
            _paymentWorkflow.Setup(_ => _.Submit(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://www.return.url");

            // Act
            await _homeController.Index("form", "page-one", viewModel, null);

            // Assert
            _mockActionsWorkflow.Verify(_ => _.Process(page.PageActions, It.IsAny<FormSchema>(), It.IsAny<string>()), Times.Once);
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
                .WithEntry("test", "test")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).ReturnsAsync(new Behaviour { BehaviourType = EBehaviourType.SubmitAndPay });
            _paymentWorkflow.Setup(_ => _.Submit(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://www.return.url");

            // Act
            await _homeController.Index("form", "page-one", viewModel, null);

            // Assert
            _mockActionsWorkflow.Verify(_ => _.Process(page.PageActions, It.IsAny<FormSchema>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SubmitWithoutSubmission_ShouldCallSubmitWorkflow()
        {
            // Act
            await _homeController.SubmitWithoutSubmission("form");

            // Assert
            _submitWorkflow.Verify(_ => _.SubmitWithoutSubmission("form"), Times.Once);
        }

        [Fact]
        public async Task SubmitWithoutSubmission_ShouldRedirectToAction_Success()
        {
            // Act
            var result = await _homeController.SubmitWithoutSubmission("form");

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Success", viewResult.ActionName);
        }

        [Fact]
        public async Task Success_ShouldCallSuccessWorkflow()
        {
            // Act
            await _homeController.Success("form");

            // Assert
            _mockSuccessWorkflow.Verify(_ => _.Process(EBehaviourType.SubmitForm, "form"), Times.Once);
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

        [Fact]
        public async Task DataStructure_ShouldRedirectToIndex_If_DataStructureNotAllowed()
        {
            // Arrange
            _mockDataStructureConfiguration
                .Setup(_ => _.Value)
                .Returns(new DataStructureConfiguration { IsEnabled = false });

            var homeController = new HomeController(
                _pageService.Object,
                _submitWorkflow.Object,
                _paymentWorkflow.Object,
                _redirectWorkflow.Object,
                _mockFileUploadService.Object,
                _mockHostingEnv.Object,
                _mockActionsWorkflow.Object,
                _emailWorkflow.Object,
                _mockSuccessWorkflow.Object,
                _mockStructureMapper.Object,
                _mockDataStructureConfiguration.Object,
                _mockLogger.Object,
                _mockSessionHelper.Object);

            // Act
            var result = await homeController.DataStructure("test-form");

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(viewResult.RouteValues.ContainsKey("form"));
        }

        [Fact]
        public async Task DataStructure_ShouldCallStructureMapper_If_DataStructureAllowed()
        {
            // Act
            await _homeController.DataStructure("test-form");

            // Assert
            _mockStructureMapper.Verify(_ => _.CreateBaseFormDataStructure(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DataStructure_ShouldReturnDataStructureView_If_DataStructureAllowed()
        {
            // Act
            var result = await _homeController.DataStructure("test-form") as ViewResult;

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("DataStructure", result.ViewName);
        }
    }
}
