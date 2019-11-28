using System.Collections.Generic;
using form_builder.Controllers;
using form_builder.Validators;
using Xunit;
using Moq;
using StockportGovUK.AspNetCore.Gateways;
using System.Threading.Tasks;
using System;
using form_builder.Models;
using Microsoft.AspNetCore.Mvc;
using form_builder.ViewModels;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using Newtonsoft.Json;
using form_builder_tests.Builders;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Logging;
using StockportGovUK.NetStandard.Models.Addresses;
using form_builder.Providers.Address;
using form_builder.Helpers.Session;

namespace form_builder_tests.UnitTests.Controllers
{
    public class AddressControllerTests
    {
        private AddressController _controller;
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

            _controller = new AddressController(_logger.Object, _mockDistributedCache.Object, _validators.Object, _schemaProvider.Object, _gateWay.Object, _pageHelper.Object, _mockAddressProviderList.Object, _mockSession.Object);
        }

        [Fact]
        public async Task Index_Get_ShouldSetAddressStatusToSearch()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var result = await _controller.Index("form", "page-one");

            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);

            Assert.Equal("Search", viewModel.AddressStatus);
        }

        [Theory]
        [InlineData(false, "Select")]
        [InlineData(true, "Search")]
        public async Task Index_Post_ShouldCallAddressProvider_WhenCorrectJourney(bool isValid, string journey)
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

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(new ValidationResult { IsValid = isValid });

            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId(questionId)
               .WithAddressProvider("testAddressProvider")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", journey)
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);

            _mockAddressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Index_Post_Should_Provide_SearchResults_ToPageHelper()
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
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()))
                .Callback<Page, Dictionary<string, string>, FormSchema, string, List<AddressSearchResult>>((x, y, z, r, w) => searchResultsCallback = w)
                .ReturnsAsync(new FormBuilderViewModel());

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Search")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);

            _mockAddressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Once);
            Assert.NotNull(searchResultsCallback);
            Assert.Single(searchResultsCallback);
            Assert.Equal(SearchReusltsUniqueId, searchResultsCallback[0].UniqueId);
            Assert.Equal(SearchReusltsName, searchResultsCallback[0].Name);
        }

        [Fact]
        public async Task Index_Post_Application_ShouldThrowApplicationException_WhenNoMatchingAddressProvider()
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
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Search")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _controller.Index("form", "page-one", viewModel));
            _mockAddressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            Assert.Equal($"No address provider configure for {addressProvider}", result.Message);
        }

        [Fact]
        public async Task Index_Post_Application_ShouldThrowApplicationException_WhenAddressProvider_ThrowsException()
        {

            _mockAddressProvider.Setup(_ => _.SearchAsync(It.IsAny<string>()))
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
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Search")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _controller.Index("form", "page-one", viewModel));

            _mockAddressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Never);
            Assert.StartsWith($"AddressController: An exception has occured while attempting to perform postcode lookup, Exception: ", result.Message);
        }

        [Fact]
        public async Task Index_Post_ShouldReturnView_WhenPageIsInvalid()
        {
            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(new ValidationResult { IsValid = false });

            var searchResultsCallback = new List<AddressSearchResult>();
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithAddressProvider("testAddressProvider")
               .WithQuestionId("test-address")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Search")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewResultModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Once);
            Assert.Equal("Search", viewResultModel.AddressStatus);
        }

        [Fact]
        public async Task Index_Post_Should_CallGenerateHtml_AndReturnView_WhenSuccessfulSearchJourney()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address")
               .WithAddressProvider("testAddressProvider")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Search")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);

            _mockAddressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Once);

            var viewResult = Assert.IsType<ViewResult>(result);
            var viewResultModel = Assert.IsType<FormBuilderViewModel>(viewResult.Model);
            Assert.Equal("Select", viewResultModel.AddressStatus);
        }

        [Fact]
        public async Task Index_Post_ApplicationShould_ThrowApplicationException_WhenPageHelper_ThrowsException()
        {
            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()))
                .Throws<Exception>();

            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address")
               .WithAddressProvider("testAddressProvider")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Search")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _controller.Index("form", "page-one", viewModel));

            _mockAddressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Once);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Once);
            Assert.StartsWith("AddressController: An exception has occured while attempting to generate Html, Exception: ", result.Message);
        }


        [Theory]
        [InlineData("Submit", EBehaviourType.SubmitForm)]
        [InlineData("Index", EBehaviourType.GoToPage)]
        public async Task Index_Post_Should_PerformRedirectToAction_WhenPageIsValid_And_SelectJourney_OnBehaviour(string viewName, EBehaviourType behaviourType)
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address")
               .WithAddressProvider("testAddressProvider")
               .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(behaviourType)
                .WithPageSlug("url")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Select")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);

            _mockAddressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Never);

            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(viewName, viewResult.ActionName);
            Assert.Equal("Home", viewResult.ControllerName);
        }

        [Fact]
        public async Task Index_Post_Should_PerformGoToExternalPageBehaviour_WhenPageIsValid_And_SelectJourney()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address")
               .WithAddressProvider("testAddressProvider")
               .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .WithPageSlug("submit-url")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new ViewModelBuilder()
                .WithEntry("Guid", Guid.NewGuid().ToString())
                .WithEntry("AddressStatus", "Select")
                .WithEntry($"{element.Properties.QuestionId}-postcode", "SK11aa")
                .Build();

            var result = await _controller.Index("form", "page-one", viewModel);

            _mockAddressProvider.Verify(_ => _.SearchAsync(It.IsAny<string>()), Times.Never);
            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Never);

            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("submit-url", redirectResult.Url);
        }
    }
}
