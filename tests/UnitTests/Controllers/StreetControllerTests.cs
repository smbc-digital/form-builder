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
using StockportGovUK.NetStandard.Models.Addresses;
using form_builder.Helpers.Session;
using form_builder.Providers.Street;

namespace form_builder_tests.UnitTests.Controllers
{
    public class StreetControllerTests
    {
        private StreetController _controller;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IEnumerable<IStreetProvider>> _mockStreetProviderList = new Mock<IEnumerable<IStreetProvider>>();
        private readonly Mock<IStreetProvider> _mockStreetProvider = new Mock<IStreetProvider>();
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private readonly Mock<IElementValidator> _testValidator = new Mock<IElementValidator>();
        private readonly Mock<ISchemaProvider> _schemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IGateway> _gateWay = new Mock<IGateway>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<ILogger<HomeController>> _logger = new Mock<ILogger<HomeController>>();
        private readonly Mock<ISessionHelper> _mockSession = new Mock<ISessionHelper>();

        private const string SearchResultsUniqueId = "123456";
        private const string SearchResultsReference = "Test street";

        public StreetControllerTests()
        {
            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(new ValidationResult { IsValid = true });

            _mockStreetProvider.Setup(_ => _.ProviderName)
                .Returns("testStreetProvider");

            _mockStreetProvider.Setup(_ => _.SearchAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<StockportGovUK.NetStandard.Models.Models.Verint.Street> { new StockportGovUK.NetStandard.Models.Models.Verint.Street { USRN = SearchResultsUniqueId, Reference = SearchResultsReference } });

            var streetProviderItems = new List<IStreetProvider> { _mockStreetProvider.Object };
            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _mockStreetProviderList.Setup(m => m.GetEnumerator()).Returns(() => streetProviderItems.GetEnumerator());
            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            _mockDistributedCache = new Mock<IDistributedCacheWrapper>();

            var guid = Guid.NewGuid();

            _mockSession.Setup(_ => _.GetSessionGuid())
                .Returns(guid.ToString());

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>()))
             .ReturnsAsync(new FormBuilderViewModel());

            var cacheData = new FormAnswers
            {
                Path = "page-one"
            };

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            _controller = new StreetController(_logger.Object, _mockDistributedCache.Object, _validators.Object, _schemaProvider.Object, _gateWay.Object, _pageHelper.Object, _mockStreetProviderList.Object, _mockSession.Object);
        }

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

            var result = await _controller.Index("form", "page-one");

            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);

            Assert.Equal("Search", viewModel.StreetStatus);
        }

        [Theory]
        [InlineData(false, "Select")]
        [InlineData(true, "Search")]
        public async Task Index_Post_ShouldCallStreetProvider_WhenCorrectJourney(bool isValid, string journey)
        {
            var questionId = "test-street";

            var cacheData = new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>()
                {
                    new PageAnswers
                    {
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = $"{questionId}-street",
                                Response = "test street"
                            }
                        },
                        PageSlug = "page-one"
                    }
                }
            };
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(new ValidationResult { IsValid = isValid });

            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId(questionId)
               .WithStreetProvider("testStreetProvider")
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

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("StreetStatus", journey)
                .WithEntry($"{element.Properties.QuestionId}-street", "test street")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);

            _mockStreetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Index_Post_Should_Provide_SearchResults_ToPageHelper()
        {
            var searchResultsCallback = new List<StockportGovUK.NetStandard.Models.Models.Verint.Street>();
            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId("test-street")
               .WithStreetProvider("testStreetProvider")
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

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>()))
                .Callback<Page, Dictionary<string, string>, FormSchema, string, List<AddressSearchResult>, List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>((x, y, z, r, p, w) => searchResultsCallback = w)
                .ReturnsAsync(new FormBuilderViewModel());

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("StreetStatus", "Search")
                .WithEntry($"{element.Properties.QuestionId}-street", "Test street")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);

            _mockStreetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>()), Times.Once);
            Assert.NotNull(searchResultsCallback);
            Assert.Single(searchResultsCallback);
            Assert.Equal(SearchResultsUniqueId, searchResultsCallback[0].USRN);
            Assert.Equal(SearchResultsReference, searchResultsCallback[0].Reference);
        }

        [Fact]
        public async Task Index_Post_ShouldReturnView_WhenPageIsInvalid()
        {
            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(new ValidationResult { IsValid = false });

            var searchResultsCallback = new List<AddressSearchResult>();
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithStreetProvider("testStreetProvider")
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

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("StreetStatus", "Search")
                .WithEntry($"{element.Properties.QuestionId}-street", "Test street")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewResultModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>()), Times.Once);
            Assert.Equal("Search", viewResultModel.StreetStatus);
        }

        [Fact]
        public async Task Index_Post_Should_CallGenerateHtml_AndReturnView_WhenSuccessfulSearchJourney()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId("test-street")
               .WithStreetProvider("testStreetProvider")
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

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("StreetStatus", "Search")
                .WithEntry($"{element.Properties.QuestionId}-street", "test street")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);

            _mockStreetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>()), Times.Once);

            var viewResult = Assert.IsType<ViewResult>(result);
            var viewResultModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);
            Assert.Equal("Select", viewResultModel.StreetStatus);
        }

        [Theory]
        [InlineData("Submit", EBehaviourType.SubmitForm)]
        [InlineData("Index", EBehaviourType.GoToPage)]
        public async Task Index_Post_Should_PerformRedirectToAction_WhenPageIsValid_And_SelectJourney_OnBehaviour(string viewName, EBehaviourType behaviourType)
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId("test-street")
               .WithStreetProvider("testStreetProvider")
               .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(behaviourType)
                .WithPageSlug("url")
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

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("StreetStatus", "Select")
                .WithEntry($"{element.Properties.QuestionId}-street", "test street")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);

            _mockStreetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>()), Times.Never);

            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(viewName, viewResult.ActionName);
            Assert.Equal("Home", viewResult.ControllerName);
        }

        [Fact]
        public async Task Index_Post_Should_PerformGoToExternalPageBehaviour_WhenPageIsValid_And_SelectJourney()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address")
               .WithStreetProvider("testStreetProvider")
               .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .WithPageSlug("submit-url")
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

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("StreetStatus", "Select")
                .WithEntry($"{element.Properties.QuestionId}-street", "test street")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);

            _mockStreetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>()), Times.Never);

            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("submit-url", redirectResult.Url);
        }
    }
}
