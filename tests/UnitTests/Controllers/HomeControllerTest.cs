using System.Collections.Generic;
using form_builder.Controllers;
using Xunit;
using Moq;
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
using StockportGovUK.NetStandard.Models.Addresses;
using form_builder.Helpers.Session;
using form_builder.Services.PageService;
using form_builder.Services.SubmtiService;
using form_builder.Services.PageService.Entities;
using form_builder.Services.SubmitService.Entities;

namespace form_builder_tests.UnitTests.Controllers
{
    public class HomeControllerTest
    {
        private HomeController _homeController;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<ISchemaProvider> _schemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IPageService> _pageService = new Mock<IPageService>();
        private readonly Mock<ISubmitService> _submitService = new Mock<ISubmitService>();
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

            _homeController = new HomeController(_logger.Object, _mockDistributedCache.Object, _schemaProvider.Object, _pageHelper.Object, _mockSession.Object, _pageService.Object, _submitService.Object);
        }

        [Fact]
        public async Task Index_ShouldCallSchemaProvider_ToGetFormSchema()
        {
            // Act
            await Assert.ThrowsAsync<NullReferenceException>(() => _homeController.Index("form", "page-one"));

            // Assert
            _schemaProvider.Verify(_ => _.Get<FormSchema>(It.Is<string>(x => x == "form")));
        }


        [Fact]
        public async Task Index_ShouldReturnAddressView_WhenTypeContainsAddress()
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
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("../Address/Index", viewResult.ViewName);
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
        public async Task Index_Application_ShoudlThrowNullException_WhenPageIsNotWithin_FormSchema()
        {
            // Arrange
            var requestPath = "non-existance-page";

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
            var result = await Assert.ThrowsAsync<NullReferenceException>(() => _homeController.Index("form", requestPath));
            Assert.Equal($"Requested path '{requestPath}' object could not be found.", result.Message);
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

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity { Page = page });

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

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Arrange
            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
               .ReturnsAsync(schema);

            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity { Page = page });

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

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
               .ReturnsAsync(schema);

            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity { Page = page });

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

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity { Page = page });

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
               .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, string[]>();

            // Act & Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _homeController.Index("form", "page-one", viewModel));
            Assert.Equal($"The provided behaviour type 'Unknown' is not valid", result.Message);

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

        [Fact]
        public async Task Index_Get_ShouldSetAddressStatusToSearch()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address")
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

            var result = await _homeController.Index("form", "page-one");

            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);

            Assert.Equal("Search", viewModel.AddressStatus);
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

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity { Page = page, ViewName = "Search", UseGeneratedViewModel = true  });

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

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Select")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity { Page = page });

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

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Select")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            _pageService.Setup(_ => _.ProcessPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()))
                .ReturnsAsync(new ProcessPageEntity { Page = page });

            var result = await _homeController.Index("form", "page-one", viewModel);

            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Never);

            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("submit-url", redirectResult.Url);
        }

        //[Fact]
        //public async Task AddressManual_Should_Return_index()
        //{
        //    var element = new ElementBuilder()
        //           .WithType(EElementType.Address)
        //           .WithQuestionId("test-address")
        //           .Build();

        //    var page = new PageBuilder()
        //        .WithElement(element)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    var result = await _homeController.Index("form", "page-one");

        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var viewModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);

        //    Assert.Equal("Search", viewModel.AddressStatus);
        //}

        [Fact]
        public async Task Index_Get_ShouldSetStreetStatusToSearch()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId("test-street")
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

            var result = await _homeController.Index("form", "page-one");

            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);

            Assert.Equal("Search", viewModel.StreetStatus);
            Assert.Equal("../Street/Index", viewResult.ViewName);
        }

    }
}
