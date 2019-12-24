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
using form_builder.Services.SubmtiService;
using form_builder.Services.PageService.Entities;
using form_builder.Services.SubmitService.Entities;
using form_builder.Models;

namespace form_builder_tests.UnitTests.Controllers
{
    public class HomeControllerTest
    {
        private HomeController _homeController;
        private readonly Mock<IPageService> _pageService = new Mock<IPageService>();
        private readonly Mock<ISubmitService> _submitService = new Mock<ISubmitService>();

        public HomeControllerTest()
        {
            _homeController = new HomeController(_pageService.Object, _submitService.Object);
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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.GoToExternalPage, PageSlug = "https://www.bbc.co.uk/weather/2636882" });

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
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();


            // Arrange
            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.GoToPage, PageSlug = "page-two" });

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
                .WithValidatedModel(true)
                .WithBehaviour(behaviour)
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.SubmitForm });

            var viewModel = new Dictionary<string, string[]>();

            // Act
            var result = await _homeController.Index("form", "page-one", viewModel);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Submit", viewResult.ActionName);
        }

        [Fact]
        public async Task Index_Application_ShoudlThrowApplicationException_ShouldRunDefaultBehaviour()
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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.Unknown });

            var viewModel = new Dictionary<string, string[]>();

            // Act & Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _homeController.Index("form", "page-one", viewModel));
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

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page, ViewName = "Search", UseGeneratedViewModel = true });

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Search")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await _homeController.Index("form", "page-one", viewModel);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Search", viewResult.ViewName);
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
                .WithEntry("AddressStatus", "Select")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = behaviourType });

            var result = await _homeController.Index("form", "page-one", viewModel);

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
                .WithEntry("AddressStatus", "Select")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            _pageService.Setup(_ => _.ProcessRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity { Page = page });
            _pageService.Setup(_ => _.GetBehaviour(It.IsAny<ProcessRequestEntity>())).Returns(new Behaviour { BehaviourType = EBehaviourType.GoToExternalPage, PageSlug = "submit-url" });

            var result = await _homeController.Index("form", "page-one", viewModel);

            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("submit-url", redirectResult.Url);
        }

        [Fact]
        public async Task Index_ShouldCallPageService()
        {
            //Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity());

            //Act
            var result = await _homeController.Index("form", "path");


            //Assert
            _pageService.Verify(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task Index_ShouldReturnViewResult()
        {
            //Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
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
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity {  ShouldRedirect = true });

            //Act
            var result = await _homeController.Index("form", "path");

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task AddressManual_ShouldCallPageService()
        {
            //Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity());

            //Act
            var result = await _homeController.AddressManual("form", "path");


            //Assert
            _pageService.Verify(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task AddressManual_ShouldReturnViewResult()
        {
            //Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity());

            //Act
            var result = await _homeController.AddressManual("form", "path");

            //Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task AddressManual_ShouldRedirect_WhenOnRedirectIsTrue()
        {
            //Arrange
            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity { ShouldRedirect = true });

            //Act
            var result = await _homeController.AddressManual("form", "path");

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Submit_ShouldReturnView_OnSuccessfull()
        {
            // Arrange
            _submitService.Setup(_ => _.ProcessSubmission(It.IsAny<string>())).ReturnsAsync(new SubmitServiceEntity { ViewName = "Success" });

            // Act
            var result = await _homeController.Submit("form");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            _submitService.Verify(_ => _.ProcessSubmission(It.IsAny<string>()), Times.Once);
            Assert.Equal("Success", viewResult.ViewName);
        }
    }
}
