using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.OrganisationService;
using form_builder_tests.Builders;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Models.Enums;
using Xunit;
using StockportGovUK.NetStandard.Models.Organisation;
using form_builder.Builders;
using form_builder.Providers.Organisation;

namespace form_builder_tests.UnitTests.Services 
{
    public class OrganisationServiceTests
    {
        private readonly OrganisationService _service;
        private OrganisationSearch _searchModel;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<IOrganisationProvider> _organisationProvider = new Mock<IOrganisationProvider>();
        private IEnumerable<IOrganisationProvider> _organisatioProviders;

        public OrganisationServiceTests()
        {
            _organisationProvider.Setup(_ => _.ProviderName).Returns("Fake");
            _organisatioProviders = new List<IOrganisationProvider>
            {
                _organisationProvider.Object
            };


            _service = new OrganisationService(_mockDistributedCache.Object, _organisatioProviders, _pageHelper.Object);

            _searchModel = new OrganisationSearch
            {
                SearchTerm = "test",
                OrganisationProvider = EOrganisationProvider.Fake
            };
        }

        [Fact]
        public async Task ProcessOrganisation_ShouldCallOrganisationProvider_WhenCorrectJourney()
        {
            var questionId = "test-org";

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

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Organisation)
               .WithQuestionId(questionId)
               .WithOrganisationProvider(EOrganisationProvider.Fake.ToString())
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

            var result = await _service.ProcessOrganisation(viewModel, page, schema, "", "page-one");

            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessOrganisation_ShouldNotCallOrganisationProvider_WhenOrganisationIsOptional()
        {
            var questionId = "test-org";

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

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Organisation)
               .WithQuestionId(questionId)
               .WithOrganisationProvider("testOrgProvider")
               .WithOptional(true)
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

            var result = await _service.ProcessOrganisation(viewModel, page, schema, "", "page-one");

            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessOrganisation_Should_NotCall_OrganisationProvider_When_SearchTerm_IsTheSame()
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
                                QuestionId = "test-address",
                                Response = _searchModel.SearchTerm
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

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Organisation)
               .WithOrganisationProvider(EOrganisationProvider.Fake.ToString())
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

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "subPath", "" },
                { element.Properties.QuestionId, _searchModel.SearchTerm },
            };

            var result = await _service.ProcessOrganisation(viewModel, page, schema, "", "page-one");

            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            _pageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()), Times.Never);
            _pageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        
        [Fact]
        public async Task ProcessOrganisation_Should_Call_OrganisationProvider_When_SearchTerm_IsDifferent()
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
                                QuestionId = "test-address",
                                Response = "old search term"
                            }
                        },
                        PageSlug = "page-one"
                    }
                }
            };

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Organisation)
               .WithOrganisationProvider(EOrganisationProvider.Fake.ToString())
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

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "subPath", "" },
                { element.Properties.QuestionId, _searchModel.SearchTerm },
            };

            var result = await _service.ProcessOrganisation(viewModel, page, schema, "", "page-one");

            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>()), Times.Once);
            _pageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessOrganisation_Application_ShouldThrowApplicationException_WhenOrganisationProvider_ThrowsException()
        {
            _organisationProvider.Setup(_ => _.SearchAsync(It.IsAny<string>())).Throws<Exception>();

            var element = new ElementBuilder()
               .WithType(EElementType.Organisation)
               .WithOrganisationProvider(EOrganisationProvider.Fake.ToString())
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

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "subPath", "" },
                { element.Properties.QuestionId, _searchModel.SearchTerm },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessOrganisation(viewModel, page, schema, "", "page-one"));
            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()), Times.Never);
            Assert.StartsWith($"OrganisationService.ProccessInitialOrganisation:: An exception has occured while attempting to perform organisation lookup, Exception: ", result.Message);
        }
    }
}
