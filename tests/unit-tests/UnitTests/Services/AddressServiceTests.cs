using form_builder.Builders;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.Address;
using form_builder.Providers.StorageProvider;
using form_builder.Services.AddressService;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class AddressServiceTests
    {
        private readonly AddressService _service;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new();
        private readonly Mock<IPageHelper> _pageHelper = new();
        private readonly Mock<IAddressProvider> _addressProvider = new();
        private readonly Mock<IPageFactory> _mockPageContentFactory = new();
        private readonly IEnumerable<IAddressProvider> _addressProviders;
        private readonly Mock<ILogger<AddressService>> _mockLogger = new();

        public AddressServiceTests()
        {
            _addressProvider.Setup(_ => _.ProviderName).Returns("testAddressProvider");
            _addressProviders = new List<IAddressProvider>
            {
                _addressProvider.Object
            };

            _service = new AddressService(_mockDistributedCache.Object, _pageHelper.Object, _addressProviders, _mockPageContentFactory.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ProcessAddress_ShouldCallAddressProvider_WhenCorrectJourney()
        {
            var questionId = "test-address";

            var cacheData = new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>()
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
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
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            await _service.ProcessAddress(viewModel, page, schema, "", "page-one");

            _addressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task ProcessAddress_ShouldNotCallAddressProvider_WhenAddressIsOptional()
        {
            var questionId = "test-address";

            var cacheData = new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>()
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
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
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-postcode", "" },
            };

            await _service.ProcessAddress(viewModel, page, schema, "", "page-one");

            _addressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessAddress_ShouldCall_PageHelper_ToProcessSearchResults()
        {
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

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            await _service.ProcessAddress(viewModel, page, schema, "", "page-one");

            _addressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact(Skip = "temp skip")]
        public async Task ProcessAddress_Application_ShouldThrowApplicationException_WhenNoMatchingAddressProvider()
        {
            var addressProvider = "NON-EXIST-PROVIDER";
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

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessAddress(viewModel, page, schema, "", "page-one"));
            _addressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            Assert.StartsWith($"AddressService::ProcessSearchAddress, An exception has occurred while attempting to perform postcode lookup on Provider '{addressProvider}' with searchterm 'SK11aa' Exception: ", result.Message);
        }

        [Fact(Skip = "temp skip")]
        public async Task ProcessAddress_Application_ShouldThrowApplicationException_WhenAddressProvider_ThrowsException()
        {
            _addressProvider.Setup(_ => _.SearchAsync(It.IsAny<string>()))
                .Throws<Exception>();

            var testAddressProvider = "testAddressProvider";

            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithAddressProvider(testAddressProvider)
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
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessAddress(viewModel, page, schema, "", "page-one"));

            _addressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Never);
            Assert.StartsWith($"AddressService::ProcessSearchAddress, An exception has occurred while attempting to perform postcode lookup on Provider '{testAddressProvider}' with searchterm 'SK11aa' Exception:", result.Message);
        }
    }
}