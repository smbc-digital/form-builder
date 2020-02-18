using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.AddressService;
using form_builder.Services.PageService;
using form_builder.Services.PageService.Entities;
using form_builder.Services.StreetService;
using form_builder.Validators;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using form_builder.Services.OrganisationService;
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;

namespace form_builder_tests.UnitTests.Services
{
    public class PageServicesTests
    {
        private readonly PageService _service;
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private readonly Mock<IElementValidator> _validator = new Mock<IElementValidator>();
        private readonly Mock<ISchemaProvider> _schemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<ISessionHelper> _sessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<ILogger<PageService>> _logger = new Mock<ILogger<PageService>>();
        private readonly Mock<IStreetService> _streetService = new Mock<IStreetService>();
        private readonly Mock<IAddressService> _addressService = new Mock<IAddressService>();
        private readonly Mock<IOrganisationService> _organisationService = new Mock<IOrganisationService>();
        private readonly Mock<IDistributedCacheWrapper> _distributedCache = new Mock<IDistributedCacheWrapper>();

        public PageServicesTests()
        {
            _validator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Returns(new ValidationResult { IsValid = false });
            var elementValidatorItems = new List<IElementValidator> { _validator.Object };
            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()))
                        .ReturnsAsync(new FormBuilderViewModel());

            var cacheData = new FormAnswers
            {
                Path = "page-one"
            };

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            _service = new PageService(_logger.Object, _validators.Object, _schemaProvider.Object, _pageHelper.Object, _sessionHelper.Object, _addressService.Object, _streetService.Object, _organisationService.Object, _distributedCache.Object);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCall_Schema_And_Session_Service()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("test-question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "AddressStatus", "Search" },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await _service.ProcessRequest("form", "page-one", viewModel, null, false);

            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()), Times.Once);
            _schemaProvider.Verify(_ => _.Get<FormSchema>(It.IsAny<string>()), Times.Once);
            _sessionHelper.Verify(_ => _.GetSessionGuid(), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCallAddressService_WhenAddressElement()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _addressService.Setup(_ => _.ProcesssAddress(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProcessRequestEntity());

            var element = new ElementBuilder()
               .WithType(EElementType.Address)
               .WithQuestionId("test-address-question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "AddressStatus", "Search" },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await _service.ProcessRequest("form", "page-one", viewModel, null, false);

            _addressService.Verify(_ => _.ProcesssAddress(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCallStreetService_WhenStreetElement()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _streetService.Setup(_ => _.ProcessStreet(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProcessRequestEntity());

            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId("test-street-question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "AddressStatus", "Search" },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await _service.ProcessRequest("form", "page-one", viewModel, null, false);

            _streetService.Verify(_ => _.ProcessStreet(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessRequest_ApplicationShould_ThrowApplicationException_WhenGenerateHtml_ThrowsException()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");

            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()))
                .Throws<ApplicationException>();

            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("test-question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "AddressStatus", "Search" },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessRequest("form", "page-one", viewModel, null, false));

            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessRequest_ApplicationShould_ThrowNullException_WhenNoSessionGUid()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("test-question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "AddressStatus", "Search" },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await Assert.ThrowsAsync<NullReferenceException>(() => _service.ProcessRequest("form", "page-one", viewModel, null, false));
            Assert.Equal("Session guid null.", result.Message);
        }

        [Fact]
        public async Task ProcessPage_ShouldCallSchemaProvider_ToGetFormSchema()
        {
            // Act
            await Assert.ThrowsAsync<NullReferenceException>(() => _service.ProcessPage("form", "page-one", false));

            // Assert
            _schemaProvider.Verify(_ => _.Get<FormSchema>(It.Is<string>(x => x == "form")));
        }

        [Fact]
        public async Task ProcessPage_ShouldReturnAddressView_WhenTypeContainsAddress()
        {
            var element = new ElementBuilder()
                 .WithType(EElementType.Address)
                 .WithQuestionId("address-test")
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

            //.Setup(_ => _.GetViewModel(It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()))
            //   .ReturnsAsync(new FormBuilderViewModel());

            // Act
            var result = await _service.ProcessPage("form", "page-one", false);

            // Assert
            var entityResult = Assert.IsType<ProcessPageEntity>(result);
            Assert.Equal("../Address/Index", entityResult.ViewName);
        }

        [Fact]
        public async Task ProcessPage_ShouldGenerateGuidWhenGuidIsEmpty()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString();
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(string.Empty);

            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
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

            // Act
            var result = await _service.ProcessPage("form", "page-one");

            // Assert
            var viewResult = Assert.IsType<ProcessPageEntity>(result);

            _sessionHelper.Verify(_ => _.GetSessionGuid(), Times.Once);
            _sessionHelper.Verify(_ => _.SetSessionGuid(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPage_Application_ShoudlThrowNullException_WhenPageIsNotWithin_FormSchema()
        {
            // Arrange
            var requestPath = "non-existance-page";

            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
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

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPage("form", requestPath));
            Assert.Equal($"Requested path '{requestPath}' object could not be found.", result.Message);
        }

        [Fact]
        public async Task ProcessPage_Get_ShouldSetAddressStatusToSearch()
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

            var result = await _service.ProcessPage("form", "page-one");

            var viewResult = Assert.IsType<ProcessPageEntity>(result);
            var viewModel = Assert.IsType<FormBuilderViewModel>(viewResult.ViewModel);

            Assert.Equal("Search", viewModel.AddressStatus);
        }

        [Fact]
        public async Task ProcessPage_AddressManual_Should_Return_index()
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

            var result = await _service.ProcessPage("form", "page-one");

            var viewResult = Assert.IsType<ProcessPageEntity>(result);
            var viewModel = Assert.IsType<FormBuilderViewModel>(viewResult.ViewModel);

            Assert.Equal("Search", viewModel.AddressStatus);
        }

        [Fact]
        public async Task ProcessPage_Get_ShouldSetStreetStatusToSearch()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Street)
               .WithQuestionId("test-street")
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

            var result = await _service.ProcessPage("form", "page-one");

            var viewResult = Assert.IsType<ProcessPageEntity>(result);
            var viewModel = Assert.IsType<FormBuilderViewModel>(viewResult.ViewModel);

            Assert.Equal("Search", viewModel.StreetStatus);
            Assert.Equal("../Street/Index", viewResult.ViewName);
        }

        [Fact]
        public async Task ProcessPage_ShouldCallDistrbutedCache_ToDeleteSessionData_WhenNavigating_ToDifferentForm()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(new FormAnswers { FormName = "other-form" }));

            var element = new ElementBuilder()
                          .WithType(EElementType.Street)
                          .WithQuestionId("test-street")
                          .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithStartPageSlug("page-one")
                .WithBaseUrl("new-form")
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var result = await _service.ProcessPage("new-form", "page-one");

            _distributedCache.Verify(_ => _.Remove(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPage_ShouldNotCallDistrbutedCache_ToDeleteSessionData_WhenNavigating_ToDifferentForm()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(new FormAnswers { FormName = "new-form" }));

            var element = new ElementBuilder()
                          .WithType(EElementType.Street)
                          .WithQuestionId("test-street")
                          .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithStartPageSlug("page-one")
                .WithBaseUrl("new-form")
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var result = await _service.ProcessPage("new-form", "page-one");

            _distributedCache.Verify(_ => _.Remove(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetBehaviour_ShouldCallSession_And_DistributedCache()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("12345");
            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(new FormAnswers { Pages = new List<PageAnswers>() }));

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-duck")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            _service.GetBehaviour(new ProcessRequestEntity { Page = page });

            _sessionHelper.Verify(_ => _.GetSessionGuid(), Times.Once);
            _distributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCallProcesssOrganisation_WhenOrganisationElement()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _organisationService.Setup(_ => _.ProcesssOrganisation(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProcessRequestEntity());

            var element = new ElementBuilder()
               .WithType(EElementType.Organisation)
               .WithQuestionId("test-org-question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "OrganisationStatus", "Search" },
                { $"{element.Properties.QuestionId}-organisation-searchterm", "orgName" },
            };

            var result = await _service.ProcessRequest("form", "page-one", viewModel, null, false);

            _organisationService.Verify(_ => _.ProcesssOrganisation(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }
    }
}
