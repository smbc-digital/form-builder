using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.Organisation;
using form_builder.Providers.StorageProvider;
using form_builder.Services.OrganisationService;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class OrganisationServiceTests
    {
        private readonly OrganisationService _service;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<IEnumerable<IOrganisationProvider>> _mockOrganisationProvider = new Mock<IEnumerable<IOrganisationProvider>>();
        private readonly Mock<IOrganisationProvider> _organisationProvider = new Mock<IOrganisationProvider>();

        public OrganisationServiceTests()
        {
            _organisationProvider.Setup(_ => _.ProviderName).Returns("testOrgProvider");

            var organisationProviderItems = new List<IOrganisationProvider> { _organisationProvider.Object };
            _mockOrganisationProvider.Setup(m => m.GetEnumerator()).Returns(() => organisationProviderItems.GetEnumerator());

            _service = new OrganisationService(_mockDistributedCache.Object, _mockOrganisationProvider.Object, _pageHelper.Object);
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
                                QuestionId = $"{questionId}-organisation-searchterm",
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
               .WithOrganisationProvider("testOrgProvider")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, string>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "OrganisationStatus", "Search" },
                { $"{element.Properties.QuestionId}-organisation-searchterm", "searchTerm" },
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
                                QuestionId = $"{questionId}-organisation-searchterm",
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

            var viewModel = new Dictionary<string, string>
            {
               { "Guid", Guid.NewGuid().ToString() },
                { "OrganisationStatus", "Search" },
                { $"{element.Properties.QuestionId}-organisation-searchterm", "" },
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
               .WithOrganisationProvider("testOrgProvider")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            var viewModel = new Dictionary<string, string>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "OrganisationStatus", "Search" },
                { $"{element.Properties.QuestionId}-organisation-searchterm", "searchTerm" },
            };

            var result = await _service.ProcesssOrganisation(viewModel, page, schema, "", "page-one");

            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.ProcessOrganisationJourney("Search", It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<OrganisationSearchResult>>()), Times.Once);
        }

        [Fact]
        public async Task ProcesssOrganisation_Application_ShouldThrowApplicationException_WhenNoMatchingOrganisationProvider()
        {
            var organisationProvider = "NON-EXIST-PROVIDER";
            var searchResultsCallback = new List<StockportGovUK.NetStandard.Models.Models.Verint.Organisation>();
            var element = new ElementBuilder()
               .WithType(EElementType.Organisation)
               .WithOrganisationProvider(organisationProvider)
               .WithQuestionId("test-org")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, string>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "OrganisationStatus", "Search" },
                { $"{element.Properties.QuestionId}-organisation-searchterm", "searchTerm" },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcesssOrganisation(viewModel, page, schema, "", "page-one"));
            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            Assert.Equal($"OrganisationService.ProcesssOrganisation:: No address provider configure for {organisationProvider}", result.Message);
        }

        [Fact]
        public async Task ProcesssOrganisation_Application_ShouldThrowApplicationException_WhenAddressProvider_ThrowsException()
        {

            _organisationProvider.Setup(_ => _.SearchAsync(It.IsAny<string>()))
                .Throws<Exception>();

            var searchResultsCallback = new List<AddressSearchResult>();
            var element = new ElementBuilder()
               .WithType(EElementType.Organisation)
               .WithOrganisationProvider("testOrgProvider")
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

            var viewModel = new Dictionary<string, string>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "OrganisationStatus", "Search" },
                { $"{element.Properties.QuestionId}-organisation-searchterm", "searchTerm" },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcesssOrganisation(viewModel, page, schema, "", "page-one"));

            _organisationProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()), Times.Never);
            Assert.StartsWith($"OrganisationService.ProcesssOrganisation:: An exception has occured while attempting to perform organisation lookup, Exception: ", result.Message);
        }
    }
}
