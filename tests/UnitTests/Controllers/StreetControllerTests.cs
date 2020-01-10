using System.Collections.Generic;
using form_builder.Controllers;
using form_builder.Validators;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using System;
using form_builder.Models;
using form_builder.ViewModels;
using form_builder.Helpers.PageHelpers;
using Newtonsoft.Json;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Logging;
using StockportGovUK.NetStandard.Models.Addresses;
using form_builder.Helpers.Session;
using form_builder.Providers.Street;
using form_builder.Models.Elements;
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;

namespace form_builder_tests.UnitTests.Controllers
{
    public class StreetControllerTests
    {
        //private StreetController _controller;
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
                .ReturnsAsync(new List<AddressSearchResult> { new AddressSearchResult { UniqueId = SearchResultsUniqueId, Name = SearchResultsReference } });

            var streetProviderItems = new List<IStreetProvider> { _mockStreetProvider.Object };
            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _mockStreetProviderList.Setup(m => m.GetEnumerator()).Returns(() => streetProviderItems.GetEnumerator());
            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            _mockDistributedCache = new Mock<IDistributedCacheWrapper>();

            var guid = Guid.NewGuid();

            _mockSession.Setup(_ => _.GetSessionGuid())
                .Returns(guid.ToString());

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()))
             .ReturnsAsync(new FormBuilderViewModel());

            var cacheData = new FormAnswers
            {
                Path = "page-one"
            };

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            //_controller = new StreetController(_logger.Object, _mockDistributedCache.Object, _validators.Object, _schemaProvider.Object, _gateWay.Object, _pageHelper.Object, _mockStreetProviderList.Object, _mockSession.Object);
        }

        //[Fact]
        //public async Task Index_Get_ShouldSetStreetStatusToSearch()
        //{
        //    var element = new ElementBuilder()
        //       .WithType(EElementType.Street)
        //       .WithQuestionId("test-street")
        //       .Build();

        //    var page = new PageBuilder()
        //        .WithElement(element)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    var result = await _controller.Index("form", "page-one");

        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var viewModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);

        //    Assert.Equal("Search", viewModel.StreetStatus);
        //}

        //[Theory]
        //[InlineData(false, "Select")]
        //[InlineData(true, "Search")]
        //public async Task Index_Post_ShouldCallStreetProvider_WhenCorrectJourney(bool isValid, string journey)
        //{
        //    var questionId = "test-street";

        //    var cacheData = new FormAnswers
        //    {
        //        Path = "page-one",
        //        Pages = new List<PageAnswers>()
        //        {
        //            new PageAnswers
        //            {
        //                Answers = new List<Answers>
        //                {
        //                    new Answers
        //                    {
        //                        QuestionId = $"{questionId}-street",
        //                        Response = "test street"
        //                    }
        //                },
        //                PageSlug = "page-one"
        //            }
        //        }
        //    };
        //    _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

        //    _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
        //        .Returns(new ValidationResult { IsValid = isValid });

        //    var element = new ElementBuilder()
        //       .WithType(EElementType.Street)
        //       .WithQuestionId(questionId)
        //       .WithStreetProvider("testStreetProvider")
        //       .Build();

        //    var page = new PageBuilder()
        //        .WithElement(element)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    var viewModel = new ViewModelBuilder()
        //        .WithEntry("Guid", Guid.NewGuid().ToString())
        //        .WithEntry("StreetStatus", journey)
        //        .WithEntry($"{element.Properties.QuestionId}-street", "test street")
        //        .Build();

        //    var result = await _controller.Index("form", "page-one", viewModel);

        //    _mockStreetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        //}

        //[Fact]
        //public async Task Index_Post_Should_Provide_SearchResults_ToPageHelper()
        //{
        //    var searchResultsCallback = new List<AddressSearchResult>();
        //    var element = new ElementBuilder()
        //       .WithType(EElementType.Street)
        //       .WithQuestionId("test-street")
        //       .WithStreetProvider("testStreetProvider")
        //       .Build();

        //    var page = new PageBuilder()
        //        .WithElement(element)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()))
        //        .Callback<Page, Dictionary<string, string>, FormSchema, string, List<AddressSearchResult>>((x, y, z, r, p) => searchResultsCallback = p)
        //        .ReturnsAsync(new FormBuilderViewModel());

        //    var viewModel = new ViewModelBuilder()
        //        .WithEntry("Guid", Guid.NewGuid().ToString())
        //        .WithEntry("StreetStatus", "Search")
        //        .WithEntry($"{element.Properties.QuestionId}-street", "Test street")
        //        .Build();

        //    var result = await _controller.Index("form", "page-one", viewModel);

        //    _mockStreetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        //    _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Once);
        //    Assert.NotNull(searchResultsCallback);
        //    Assert.Single(searchResultsCallback);
        //    Assert.Equal(SearchResultsUniqueId, searchResultsCallback[0].UniqueId);
        //    Assert.Equal(SearchResultsReference, searchResultsCallback[0].Name);
        //}

        //[Fact]
        //public async Task Index_Post_ShouldReturnView_WhenPageIsInvalid()
        //{
        //    _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
        //        .Returns(new ValidationResult { IsValid = false });

        //    var searchResultsCallback = new List<StockportGovUK.NetStandard.Models.Models.Verint.Street>();
        //    var element = new ElementBuilder()
        //       .WithType(EElementType.Street)
        //       .WithStreetProvider("testStreetProvider")
        //       .WithQuestionId("test-street")
        //       .Build();

        //    var page = new PageBuilder()
        //        .WithElement(element)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    var viewModel = new ViewModelBuilder()
        //        .WithEntry("Guid", Guid.NewGuid().ToString())
        //        .WithEntry("StreetStatus", "Search")
        //        .WithEntry($"{element.Properties.QuestionId}-street", "Test street")
        //        .Build();

        //    var result = await _controller.Index("form", "page-one", viewModel);
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var viewResultModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);
        //    _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Once);
        //    Assert.Equal("Search", viewResultModel.StreetStatus);
        //}

        //[Fact]
        //public async Task Index_Post_Should_CallGenerateHtml_AndReturnView_WhenSuccessfulSearchJourney()
        //{
        //    var element = new ElementBuilder()
        //       .WithType(EElementType.Street)
        //       .WithQuestionId("test-street")
        //       .WithStreetProvider("testStreetProvider")
        //       .Build();

        //    var page = new PageBuilder()
        //        .WithElement(element)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    var viewModel = new ViewModelBuilder()
        //        .WithEntry("Guid", Guid.NewGuid().ToString())
        //        .WithEntry("StreetStatus", "Search")
        //        .WithEntry($"{element.Properties.QuestionId}-street", "test street")
        //        .Build();

        //    var result = await _controller.Index("form", "page-one", viewModel);

        //    _mockStreetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        //    _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Once);

        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var viewResultModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);
        //    Assert.Equal("Select", viewResultModel.StreetStatus);
        //}
    }
}