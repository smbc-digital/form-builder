using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.Address;
using form_builder.Providers.StorageProvider;
using form_builder.Services.AddressService;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class AddressServiceTests
    {
        private readonly AddressService _service;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<IEnumerable<IAddressProvider>> _mockAddressProvider = new Mock<IEnumerable<IAddressProvider>>();
        private readonly Mock<IAddressProvider> _addressProvider = new Mock<IAddressProvider>();

        public AddressServiceTests()
        {
            _addressProvider.Setup(_ => _.ProviderName).Returns("testAddressProvider");

            var addressProviderItems = new List<IAddressProvider> { _addressProvider.Object };
            _mockAddressProvider.Setup(m => m.GetEnumerator()).Returns(() => addressProviderItems.GetEnumerator());

            _service = new AddressService(_mockDistributedCache.Object, _mockAddressProvider.Object, _pageHelper.Object);
        }

        [Theory]
        [InlineData(true, "Search")]
        public async Task ProcesssAddress_ShouldCallAddressProvider_WhenCorrectJourney(bool isValid, string journey)
        {
            var questionId = "test-address";

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
                                QuestionId = $"{questionId}-postcode",
                                Response = "sk11aa"
                            }
                        },
                        PageSlug = "page-one"
                    }
                }
            };
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId(questionId)
               .WithAddressProvider("testAddressProvider")               
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(isValid)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, string>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "AddressStatus", journey },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await _service.ProcesssAddress(viewModel, page, schema, "", "page-one");

            _addressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        }


        [Theory]
        [InlineData(true, "Search")]
        public async Task ProcesssAddress_ShouldNotCallAddressProvider_WhenAddressIsOptional(bool isValid, string journey)
        {
            var questionId = "test-address";

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
                                QuestionId = $"{questionId}-postcode",
                                Response = ""
                            }
                        },
                        PageSlug = "page-one"
                    }
                }
            };
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId(questionId)
               .WithAddressProvider("testAddressProvider")
               .WithOptional(true)
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(isValid)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, string>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "AddressStatus", journey },
                { $"{element.Properties.QuestionId}-postcode", "" },
            };

            var result = await _service.ProcesssAddress(viewModel, page, schema, "", "page-one");

            _addressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcesssAddress_ShouldCall_PageHelper_ToProcessSearchResults()
        {
            var searchResultsCallback = new List<AddressSearchResult>();
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address")
               .WithAddressProvider("testAddressProvider")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()))
                .Callback<Page, Dictionary<string, string>, FormSchema, string, List<AddressSearchResult>>((x, y, z, r, w) => searchResultsCallback = w)
                .ReturnsAsync(new FormBuilderViewModel());

            var viewModel = new Dictionary<string, string>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "AddressStatus", "Search" },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await _service.ProcesssAddress(viewModel, page, schema, "", "page-one");

            _addressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.ProcessStreetAndAddressJourney("Search", It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task ProcesssAddress_Application_ShouldThrowApplicationException_WhenNoMatchingAddressProvider()
        {
            var addressProvider = "NON-EXIST-PROVIDER";
            var searchResultsCallback = new List<AddressSearchResult>();
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithAddressProvider(addressProvider)
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
                { "AddressStatus", "Search" },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcesssAddress(viewModel, page, schema, "", "page-one"));
            _addressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            Assert.Equal($"No address provider configure for {addressProvider}", result.Message);
        }

        [Fact]
        public async Task ProcesssAddress_Application_ShouldThrowApplicationException_WhenAddressProvider_ThrowsException()
        {

            _addressProvider.Setup(_ => _.SearchAsync(It.IsAny<string>()))
                .Throws<Exception>();

            var searchResultsCallback = new List<AddressSearchResult>();
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithAddressProvider("testAddressProvider")
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
                { "AddressStatus", "Search" },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcesssAddress(viewModel, page, schema, "", "page-one"));

            _addressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Never);
            Assert.StartsWith($"AddressController: An exception has occured while attempting to perform postcode lookup, Exception: ", result.Message);
        }
    }
}