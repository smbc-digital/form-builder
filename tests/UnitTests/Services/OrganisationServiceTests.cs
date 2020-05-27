using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.OrganisationService;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Gateways.Response;
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
        public async Task ProcesssOrganisation_ShouldCallOrganisationProvider_WhenCorrectJourney()
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
                { "OrganisationStatus", "Search" },
                { element.Properties.QuestionId, "searchTerm" },
            };

            var result = await _service.ProcesssOrganisation(viewModel, page, schema, "", "page-one");

            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcesssOrganisation_ShouldNotCallOrganisationProvider_WhenOrganisationIsOptional()
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
                { "OrganisationStatus", "Search" },
                { element.Properties.QuestionId, "" },
            };

            var result = await _service.ProcesssOrganisation(viewModel, page, schema, "", "page-one");

            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcesssOrganisation_ShouldCall_PageHelper_ToProcessSearchResults()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Organisation)
               .WithQuestionId("test-org")
               .WithOrganisationProvider(EOrganisationProvider.Fake.ToString())
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "OrganisationStatus", "Search" },
                { element.Properties.QuestionId, "searchTerm" },
            };

            var result = await _service.ProcesssOrganisation(viewModel, page, schema, "", "page-one");

            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.ProcessOrganisationJourney("Search", It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<OrganisationSearchResult>>()), Times.Once);
        }

        [Fact]
        public async Task ProcesssOrganisation_Application_ShouldThrowApplicationException_WhenOrganisationProvider_ThrowsException()
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
                { "OrganisationStatus", "Search" },
                { element.Properties.QuestionId, _searchModel.SearchTerm },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcesssOrganisation(viewModel, page, schema, "", "page-one"));
            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()), Times.Never);
            Assert.StartsWith($"OrganisationService.ProcesssOrganisation:: An exception has occured while attempting to perform organisation lookup, Exception: ", result.Message);
        }
    }
}
