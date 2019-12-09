using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Street;
using form_builder.Services.StreetService;
using form_builder_tests.Builders;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class StreetServiceTests
    {
        private readonly StreetService _service;
        private readonly Mock<IDistributedCacheWrapper> _distributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<IEnumerable<IStreetProvider>> _streetProviders = new Mock<IEnumerable<IStreetProvider>>();
        private readonly Mock<IStreetProvider> _streetProvider = new Mock<IStreetProvider>();

        public StreetServiceTests()
        {
            _streetProvider.Setup(_ => _.ProviderName).Returns("testStreetProvider");

            var addressProviderItems = new List<IStreetProvider> { _streetProvider.Object };
            _streetProviders.Setup(m => m.GetEnumerator()).Returns(() => addressProviderItems.GetEnumerator());

            _service = new StreetService(_distributedCache.Object, _streetProviders.Object, _pageHelper.Object);
        }

        [Theory]
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

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(Newtonsoft.Json.JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId(questionId)
               .WithStreetProvider("testStreetProvider")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(isValid)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, string>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "StreetStatus", journey },
                { $"{element.Properties.QuestionId}-street", "test street" },
            };

            var result = await _service.ProcessStreet(viewModel, page, schema, "", "page-one");

            _streetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        }

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
