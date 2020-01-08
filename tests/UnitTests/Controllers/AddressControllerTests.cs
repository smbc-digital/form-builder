using System.Collections.Generic;
using form_builder.Controllers;
using form_builder.Validators;
using Moq;
using System;
using form_builder.Models;
using form_builder.ViewModels;
using form_builder.Helpers.PageHelpers;
using Newtonsoft.Json;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Logging;
using StockportGovUK.NetStandard.Models.Addresses;
using form_builder.Providers.Address;
using form_builder.Helpers.Session;
using form_builder.Models.Elements;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder_tests.UnitTests.Controllers
{
    public class AddressControllerTests
    {
        // private AddressController _controller;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IEnumerable<IAddressProvider>> _mockAddressProviderList = new Mock<IEnumerable<IAddressProvider>>();
        private readonly Mock<IAddressProvider> _mockAddressProvider = new Mock<IAddressProvider>();
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private readonly Mock<IElementValidator> _testValidator = new Mock<IElementValidator>();
        private readonly Mock<ISchemaProvider> _schemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IGateway> _gateWay = new Mock<IGateway>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<ILogger<HomeController>> _logger = new Mock<ILogger<HomeController>>();
        private readonly Mock<ISessionHelper> _mockSession = new Mock<ISessionHelper>();

        private const string SearchReusltsUniqueId = "123456";
        private const string SearchReusltsName = "name, street, county";

        public AddressControllerTests()
        {
            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(new ValidationResult { IsValid = true });

            _mockAddressProvider.Setup(_ => _.ProviderName)
                .Returns("testAddressProvider");

            _mockAddressProvider.Setup(_ => _.SearchAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<AddressSearchResult> { new AddressSearchResult { UniqueId = SearchReusltsUniqueId, Name = SearchReusltsName } });

            var addressProviderItems = new List<IAddressProvider> { _mockAddressProvider.Object };
            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _mockAddressProviderList.Setup(m => m.GetEnumerator()).Returns(() => addressProviderItems.GetEnumerator());
            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            _mockDistributedCache = new Mock<IDistributedCacheWrapper>();

            var guid = Guid.NewGuid();

            _mockSession.Setup(_ => _.GetSessionGuid())
                .Returns(guid.ToString());

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()))
             .ReturnsAsync(new FormBuilderViewModel());

            var cacheData = new FormAnswers
            {
                Path = "page-one"
            };

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            //_controller = new AddressController(_logger.Object, _mockDistributedCache.Object, _validators.Object, _schemaProvider.Object, _gateWay.Object, _pageHelper.Object, _mockAddressProviderList.Object, _mockSession.Object);
        }

    }

}