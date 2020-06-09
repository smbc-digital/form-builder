using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using form_builder.Services.StreetService;
using form_builder_tests.Builders;
using Moq;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using form_builder.Builders;
using form_builder.Providers.Street;

namespace form_builder_tests.UnitTests.Services
{
    public class StreetServiceTests
    {
        private readonly StreetService _service;
        private readonly Mock<IDistributedCacheWrapper> _distributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<IStreetProvider> _streetProvider = new Mock<IStreetProvider>();
        private IEnumerable<IStreetProvider> _streetProviders;

        private const string SearchResultsUniqueId = "123456";
        private const string SearchResultsReference = "Test street";

        public StreetServiceTests()
        {
             _streetProvider.Setup(_ => _.ProviderName).Returns("Fake");
            _streetProviders = new List<IStreetProvider>
            {
                _streetProvider.Object
            };

            _service = new StreetService(_distributedCache.Object, _streetProviders, _pageHelper.Object);
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
               .WithStreetProvider("Fake")
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
    }
}
