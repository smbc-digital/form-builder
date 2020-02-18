using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Street;
using form_builder.Services.PageService.Entities;
using form_builder.Services.StreetService;
using form_builder_tests.Builders;
using Moq;
using StockportGovUK.NetStandard.Models.Addresses;
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

        private const string SearchResultsUniqueId = "123456";
        private const string SearchResultsReference = "Test street";

        public StreetServiceTests()
        {
            _streetProvider.Setup(_ => _.ProviderName).Returns("testStreetProvider");

            var addressProviderItems = new List<IStreetProvider> { _streetProvider.Object };
            _streetProviders.Setup(m => m.GetEnumerator()).Returns(() => addressProviderItems.GetEnumerator());

            _service = new StreetService(_distributedCache.Object, _streetProviders.Object, _pageHelper.Object);
        }

        [Theory]
        [InlineData(true, "Search")]
        public async Task ProcessStreet_ShouldCallStreetProvider_WhenCorrectJourney(bool isValid, string journey)
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

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "StreetStatus", journey },
                { $"{element.Properties.QuestionId}-street", "test street" },
            };

            var result = await _service.ProcessStreet(viewModel, page, schema, "", "page-one");

            _streetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessStreet_Should_Provide_SearchResults_ToPageHelper()
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

            _streetProvider.Setup(_ => _.SearchAsync(It.IsAny<string>())).ReturnsAsync(new List<AddressSearchResult> { new AddressSearchResult { UniqueId = SearchResultsUniqueId, Name = SearchResultsReference } });

            var searchResultsCallback = new List<AddressSearchResult>();
            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId(questionId)
               .WithStreetProvider("testStreetProvider")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _pageHelper.Setup(_ => _.ProcessStreetJourney(It.Is<string>(x => x == "Search"), It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()))
                .Callback<string, Page, Dictionary<string, dynamic>, FormSchema, string, List<AddressSearchResult>>((a, x, y, z, r, p) => searchResultsCallback = p)
                .ReturnsAsync(new ProcessRequestEntity());

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "StreetStatus", "Search" },
                { $"{element.Properties.QuestionId}-street", "Test street" },
            };

            var result = await _service.ProcessStreet(viewModel, page, schema, "", "page-one");

            _streetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.ProcessStreetJourney(It.Is<string>(x => x == "Search"), It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Once);
            Assert.NotNull(searchResultsCallback);
            Assert.Single(searchResultsCallback);
            Assert.Equal(SearchResultsUniqueId, searchResultsCallback[0].UniqueId);
            Assert.Equal(SearchResultsReference, searchResultsCallback[0].Name);
        }
    }
}
