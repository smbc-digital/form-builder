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
using form_builder.Providers.Organisation;

namespace form_builder_tests.UnitTests.Controllers
{
    public class OrganisationControllerTests
    {
        //private StreetController _controller;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IEnumerable<IOrganisationProvider>> _mockOrganisationProviderList = new Mock<IEnumerable<IOrganisationProvider>>();
        private readonly Mock<IOrganisationProvider> _mockOrganisationProvider = new Mock<IOrganisationProvider>();
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private readonly Mock<IElementValidator> _testValidator = new Mock<IElementValidator>();
        private readonly Mock<ISchemaProvider> _schemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IGateway> _gateWay = new Mock<IGateway>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<ILogger<HomeController>> _logger = new Mock<ILogger<HomeController>>();
        private readonly Mock<ISessionHelper> _mockSession = new Mock<ISessionHelper>();

        private const string SearchResultsUniqueId = "123456";
        private const string SearchResultsReference = "Tesco";

        public OrganisationControllerTests()
        {
            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(new ValidationResult { IsValid = true });

            _mockOrganisationProvider.Setup(_ => _.ProviderName)
                .Returns("testOrganisationProvider");

            _mockOrganisationProvider.Setup(_ => _.SearchAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<OrganisationSearchResult> { new OrganisationSearchResult { Address = SearchResultsUniqueId, Reference = SearchResultsReference } });

            var organisationProviderItems = new List<IOrganisationProvider> { _mockOrganisationProvider.Object };
            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _mockOrganisationProviderList.Setup(m => m.GetEnumerator()).Returns(() => organisationProviderItems.GetEnumerator());
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
    }
}