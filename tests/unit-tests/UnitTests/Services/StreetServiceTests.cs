using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.ContentFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Street;
using form_builder.Services.StreetService;
using form_builder_tests.Builders;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Enums;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class StreetServiceTests
    {
        private readonly StreetService _service;
        private readonly Mock<IDistributedCacheWrapper> _distributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<IStreetProvider> _streetProvider = new Mock<IStreetProvider>();
        private IEnumerable<IStreetProvider> _streetProviders;
        private readonly Mock<IPageFactory> _mockPageContentFactory = new Mock<IPageFactory>();

        public StreetServiceTests()
        {
             _streetProvider.Setup(_ => _.ProviderName).Returns("Fake");
            _streetProviders = new List<IStreetProvider>
            {
                _streetProvider.Object
            };

            _service = new StreetService(_distributedCache.Object, _streetProviders, _pageHelper.Object, _mockPageContentFactory.Object);
        }

        [Fact]
        public async Task ProcessStreet_ShouldCallStreetProvider_WhenCorrectJourney()
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
                                QuestionId = questionId,
                                Response = "searchTerm"
                            }
                        },
                        PageSlug = "page-one"
                    }
                },
                FormData = new Dictionary<string, object>
                {
                    { "page-one-search-results" , new List<object>() } 
                }
            };

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId(questionId)
               .WithStreetProvider(EStreetProvider.Fake.ToString())
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "subPath", "automatic" },
                { element.Properties.QuestionId, "searchTerm" },
            };

            await _service.ProcessStreet(viewModel, page, schema, "", "page-one");

            _streetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessStreet_ShouldNotCallStreetProvider_WhenStreetIsOptional()
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
                                QuestionId = questionId,
                                Response = ""
                            }
                        },
                        PageSlug = "page-one"
                    }
                }
            };

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId(questionId)
               .WithStreetProvider(EStreetProvider.Fake.ToString())
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
               { "Guid", Guid.NewGuid().ToString() },
                { "subPath", "automatic" },
                { element.Properties.QuestionId, "" },
            };

            await _service.ProcessStreet(viewModel, page, schema, "", "page-one");

            _streetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessStreet_Should_NotCall_StreetProvider_When_SearchTerm_IsTheSame()
        {
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
                                QuestionId = "test-street",
                                Response = "streetname"
                            }
                        },
                        PageSlug = "page-one"
                    }
                },
                FormData = new Dictionary<string, object> 
                {
                    {"page-one-search-results", new List<object>{ }}
                }
            };

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId("test-street")
               .WithStreetProvider(EStreetProvider.Fake.ToString())
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "subPath", "" },
                { element.Properties.QuestionId, "streetname" },
            };

            await _service.ProcessStreet(viewModel, page, schema, "", "page-one");

            _streetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            _pageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
            _pageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        
        [Fact]
        public async Task ProcessStreet_Should_Call_StreetProvider_When_SearchTerm_IsDifferent()
        {
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
                                QuestionId = "test-street",
                                Response = "old search term"
                            }
                        },
                        PageSlug = "page-one"
                    }
                }
            };

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId("test-street")
               .WithStreetProvider(EStreetProvider.Fake.ToString())
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "subPath", "" },
                { element.Properties.QuestionId, "new street search" },
            };

            await _service.ProcessStreet(viewModel, page, schema, "", "page-one");

            _streetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _pageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessStreet_Application_ShouldThrowApplicationException_WhenStreetProvider_ThrowsException()
        {
            _streetProvider.Setup(_ => _.SearchAsync(It.IsAny<string>())).Throws<Exception>();

            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId("street")
               .WithStreetProvider(EStreetProvider.Fake.ToString())
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "subPath", "" },
                { element.Properties.QuestionId, "streetname" },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessStreet(viewModel, page, schema, "", "page-one"));
            _streetProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Never);
            Assert.StartsWith($"StreetService::ProccessInitialStreet: An exception has occured while attempting to perform street lookup, Exception: ", result.Message);
        }
    }
}