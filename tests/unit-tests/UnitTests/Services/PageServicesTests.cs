using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.ContentFactory.SuccessPageFactory;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Helpers.IncomingDataHelper;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.FileStorage;
using form_builder.Providers.StorageProvider;
using form_builder.Services.AddAnotherService;
using form_builder.Services.AddressService;
using form_builder.Services.BookingService;
using form_builder.Services.BookingService.Entities;
using form_builder.Services.FileUploadService;
using form_builder.Services.FormAvailabilityService;
using form_builder.Services.OrganisationService;
using form_builder.Services.PageService;
using form_builder.Services.PageService.Entities;
using form_builder.Services.StreetService;
using form_builder.Validators;
using form_builder.Validators.IntegrityChecks;
using form_builder.ViewModels;
using form_builder.Workflows.ActionsWorkflow;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace form_builder_tests.UnitTests.Services {
    public class PageServicesTests {
        private readonly PageService _service;
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new();
        private readonly Mock<IElementValidator> _validator = new();
        private readonly Mock<IPageHelper> _mockPageHelper = new();
        private readonly Mock<ISessionHelper> _sessionHelper = new();
        private readonly Mock<IStreetService> _streetService = new();
        private readonly Mock<IAddressService> _addressService = new();
        private readonly Mock<IBookingService> _bookingService = new();
        private readonly Mock<IFileUploadService> _fileUploadService = new();
        private readonly Mock<IOrganisationService> _organisationService = new();
        private readonly Mock<IDistributedCacheWrapper> _distributedCache = new();
        private readonly IEnumerable<IFileStorageProvider> _fileStorageProviders;
        private readonly Mock<IFileStorageProvider> _fileStorageProvider = new();
        private readonly Mock<ISchemaFactory> _mockSchemaFactory = new();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistributedCacheExpirationConfiguration = new();
        private readonly Mock<IWebHostEnvironment> _mockEnvironment = new();
        private readonly Mock<IPageFactory> _mockPageFactory = new();
        private readonly Mock<IIncomingDataHelper> _mockIncomingDataHelper = new();
        private readonly Mock<ISuccessPageFactory> _mockSuccessPageFactory = new();
        private readonly Mock<IActionsWorkflow> _mockActionsWorkflow = new();
        private readonly Mock<IFormAvailabilityService> _mockFormAvailabilityService = new();
        private readonly Mock<IFormSchemaIntegrityValidator> _mockFormSchemaIntegrityValidator = new();
        private readonly Mock<ILogger<IPageService>> _mockLogger = new();
        private readonly Mock<IConfiguration> _mockConfiguration = new();
        private readonly Mock<IAddAnotherService> _mockAddAnotherService = new();

        public PageServicesTests() {
            _mockConfiguration.Setup(_ => _["FileStorageProvider:Type"]).Returns("Redis");

            _fileStorageProvider.Setup(_ => _.ProviderName).Returns("Redis");
            _fileStorageProviders = new List<IFileStorageProvider>
            {
                _fileStorageProvider.Object
            };

            _mockFormAvailabilityService.Setup(_ => _.IsAvailable(It.IsAny<List<EnvironmentAvailability>>(), It.IsAny<string>()))
                .Returns(true);

            _validator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = false });

            var elementValidatorItems = new List<IElementValidator> { _validator.Object };
            _validators
                .Setup(m => m.GetEnumerator())
                .Returns(() => elementValidatorItems.GetEnumerator());

            _mockPageHelper
                .Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            _mockPageHelper
                .Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), null))
                .ReturnsAsync(new FormBuilderViewModel());

            _mockEnvironment
                .Setup(_ => _.EnvironmentName)
                .Returns("local");

            _mockFormSchemaIntegrityValidator
                .Setup(_ => _.Validate(It.IsAny<FormSchema>()));

            _mockElementHelper
                .Setup(_ => _.Valida)

            var cacheData = new FormAnswers {
                Path = "page-one",
                FormName = "form"
            };

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            _fileStorageProvider.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            _mockDistributedCacheExpirationConfiguration.Setup(_ => _.Value).Returns(new DistributedCacheExpirationConfiguration {
                FormJson = 1
            });

            _service = new PageService(
                _validators.Object,
                _mockPageHelper.Object,
                _sessionHelper.Object,
                _addressService.Object,
                _fileUploadService.Object,
                _streetService.Object,
                _organisationService.Object,
                _distributedCache.Object,
                _mockDistributedCacheExpirationConfiguration.Object,
                _mockEnvironment.Object,
                _mockSuccessPageFactory.Object,
                _mockPageFactory.Object,
                _bookingService.Object,
                _mockSchemaFactory.Object,
                _mockIncomingDataHelper.Object,
                _mockActionsWorkflow.Object,
                _mockAddAnotherService.Object,
                _mockFormAvailabilityService.Object,
                _mockLogger.Object, _fileStorageProviders,
                _mockConfiguration.Object,
                _mockElementHelper.Object
                );
        }

        [Fact]
        public async Task ProcessPage_ShouldReturnNull_IfFormIsNotAvailable() {
            _mockFormAvailabilityService.Setup(_ => _.IsAvailable(It.IsAny<List<EnvironmentAvailability>>(), It.IsAny<string>()))
                .Returns(false);

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("local", false)
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            var result = await _service.ProcessPage("form", "page-one", "", new QueryCollection());
            _mockLogger.Verify(_ => _.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _mockFormAvailabilityService.Verify(_ => _.IsAvailable(It.IsAny<List<EnvironmentAvailability>>(), It.IsAny<string>()), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task ProcessPage_ShouldCallSchemaFactory_ToGetFormSchema() {
            // Act
            var result = await _service.ProcessPage("form", "page-one", "", new QueryCollection());

            // Assert
            Assert.Null(result);
            _mockSchemaFactory.Verify(_ => _.Build(It.IsAny<string>(), null), Times.Once);
        }

        [Fact]
        public async Task ProcessPage_ShouldGenerateGuidWhenGuidIsEmpty() {
            // Arrange
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            // Act
            var result = await _service.ProcessPage("form", "page-one", "", new QueryCollection());

            // Assert
            Assert.IsType<ProcessPageEntity>(result);

            _sessionHelper.Verify(_ => _.GetSessionGuid(), Times.Once);
            _sessionHelper.Verify(_ => _.SetSessionGuid(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPage_Application_ShouldThrowNullException_WhenPageIsNotWithin_FormSchema() {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns((Page)null);

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPage("form", requestPath, "", new QueryCollection()));
            Assert.Equal($"Requested path '{requestPath}' object could not be found for form 'form'", result.Message);
        }

        [Fact]
        public async Task ProcessPage_AddressManual_Should_Return_index() {
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

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            var result = await _service.ProcessPage("form", "page-one", LookUpConstants.Manual, new QueryCollection());

            Assert.Equal("Index", result.ViewName);
            Assert.False(result.ShouldRedirect);
        }

        [Fact]
        public async Task ProcessPage_ShouldCallDistributedCache_ToDeleteSessionData_WhenNavigating_ToDifferentForm() {
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
                .WithFirstPageSlug("page-one")
                .WithBaseUrl("new-form")
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            await _service.ProcessPage("new-form", "page-one", "", new QueryCollection());

            _distributedCache.Verify(_ => _.Remove(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPage_ShouldNotCallDistributedCache_ToDeleteSessionData_WhenNavigating_ToDifferentForm() {
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
                .WithFirstPageSlug("page-one")
                .WithBaseUrl("new-form")
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            await _service.ProcessPage("new-form", "page-one", "", new QueryCollection());

            _distributedCache.Verify(_ => _.Remove(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPage_ShouldGetTheRightStartPageUrl_IfNoDocumentUpload() {
            //Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Textbox)
               .WithQuestionId("test-textbox")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithBaseUrl("textbox")
                .WithStartPageUrl("page-one")
                .Build();

            var viewModel = new FormBuilderViewModel {
                StartPageUrl = "https://www.test.com/textbox/page-one"
            };

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(viewModel);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            //Act
            var result = await _service.ProcessPage("form", "page-one", "", new QueryCollection());

            //Assert
            Assert.Equal(viewModel.StartPageUrl, result.ViewModel.StartPageUrl);
        }

        [Fact]
        public async Task ProcessPage_ShouldGetTheRightStartPageUrl_IfHasDocumentUpload_AndPathNotDocumentUpload() {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("test-textbox")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("file")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var documentUploadPage = new PageBuilder()
                .WithElement(element2)
                .WithPageSlug("document-upload")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(documentUploadPage)
                .WithBaseUrl("form")
                .WithStartPageUrl("page-one")
                .Build();

            var viewModel = new FormBuilderViewModel {
                StartPageUrl = "https://www.test.com/form/page-one"
            };

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(viewModel);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            //Act
            var result = await _service.ProcessPage("form", "fake-path", "", new QueryCollection());

            //Assert
            Assert.Equal(viewModel.StartPageUrl, result.ViewModel.StartPageUrl);
        }

        [Fact]
        public async Task ProcessRequest_ShouldThrowException_IfFormIsNotAvailable() {
            _mockFormAvailabilityService.Setup(_ => _.IsAvailable(It.IsAny<List<EnvironmentAvailability>>(), It.IsAny<string>()))
                .Returns(false);

            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("local", false)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessRequest("form", "page-one", viewModel, null, true));
            _mockFormAvailabilityService.Verify(_ => _.IsAvailable(It.IsAny<List<EnvironmentAvailability>>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCallAddressService_WhenAddressElement() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _addressService.Setup(_ => _.ProcessAddress(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()))
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await _service.ProcessRequest("form", "page-one", viewModel, null, true);

            _addressService.Verify(_ => _.ProcessAddress(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCallStreetService_WhenStreetElement() {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await _service.ProcessRequest("form", "page-one", viewModel, null, true);

            _streetService.Verify(_ => _.ProcessStreet(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessRequest_ApplicationShould_ThrowApplicationException_WhenGenerateHtml_ThrowsException() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");

            _mockPageFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessRequest("form", "page-one", viewModel, null, true));

            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessRequest_ApplicationShould_ThrowNullException_WhenNoSessionGUid() {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await Assert.ThrowsAsync<NullReferenceException>(() => _service.ProcessRequest("form", "page-one", viewModel, null, true));
            Assert.Equal("Session guid null.", result.Message);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCallProcessOrganisation_WhenOrganisationElement() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _organisationService.Setup(_ => _.ProcessOrganisation(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()))
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-organisation-searchterm", "orgName" },
            };

            var result = await _service.ProcessRequest("form", "page-one", viewModel, null, true);

            _organisationService.Verify(_ => _.ProcessOrganisation(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessRequest_ShouldGetTheRightStartPageUrl() {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("test-textarea")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("first-page")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithBaseUrl("textarea")
                .WithStartPageUrl("first-page")
                .Build();

            var viewModel = new FormBuilderViewModel {
                StartPageUrl = "https://www.test.com/textarea/first-page"
            };

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _sessionHelper.Setup(_ => _.GetSessionGuid())
                .Returns("guid");

            _mockPageFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(viewModel);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            //Act
            var result = await _service.ProcessRequest("form", "first-page", new Dictionary<string, dynamic>(), It.IsAny<IEnumerable<CustomFormFile>>(), true);

            //Assert
            Assert.Equal(viewModel.StartPageUrl, result.ViewModel.StartPageUrl);
        }

        [Fact]
        public async Task ProcessRequest_ShouldNot_CallPageHelper_WhenPageContains_NoInboundValues() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _mockPageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            var page = new PageBuilder()
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            await _service.ProcessRequest("form", "page-one", new Dictionary<string, dynamic>(), null, true);

            _mockIncomingDataHelper.Verify(_ => _.AddIncomingFormDataValues(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessRequest_Should_CallPageHelper_WhenPageContains_InboundValues() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _mockPageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            var incomingValue = new IncomingValuesBuilder()
                .WithHttpActionType(EHttpActionType.Post)
                .Build();

            var page = new PageBuilder()
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .WithIncomingValue(incomingValue)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            await _service.ProcessRequest("form", "page-one", new Dictionary<string, dynamic>(), It.IsAny<IEnumerable<CustomFormFile>>(), true);

            _mockIncomingDataHelper.Verify(_ => _.AddIncomingFormDataValues(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCallFileUploadService_WhenMultipleFileUploadElement() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");

            _fileUploadService
                .Setup(_ => _.ProcessFile(It.IsAny<Dictionary<string, dynamic>>(),
                    It.IsAny<Page>(),
                    It.IsAny<FormSchema>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    null,
                    It.IsAny<bool>()))
                .ReturnsAsync(new ProcessRequestEntity());

            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileUpload")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-fileupload", "file" }
            };

            var result = await _service.ProcessRequest("form", "page-one", viewModel, null, true);

            _fileUploadService.Verify(_ => _.ProcessFile(It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<Page>(),
                It.IsAny<FormSchema>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<bool>()), Times.Once());
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessRequest_ShouldNotCallFileUploadService_WhenNoMultipleFileUploadElement() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");

            _mockPageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            var page = new PageBuilder()
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            await _service.ProcessRequest("form", "page-one", new Dictionary<string, dynamic>(), null, true);

            _fileUploadService.Verify(_ => _.ProcessFile(It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<Page>(),
                It.IsAny<FormSchema>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCall_IncomingDataHelper_WhenFormHasIncomingGetValues() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(string.Empty);

            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
                .Build();

            var incomingGetValue = new IncomingValuesBuilder()
                .WithHttpActionType(EHttpActionType.Get)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithIncomingValue(incomingGetValue)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            // Act
            var result = await _service.ProcessPage("form", "page-one", "", new QueryCollection());

            // Assert
            Assert.IsType<ProcessPageEntity>(result);
            _mockIncomingDataHelper.Verify(_ => _.AddIncomingFormDataValues(It.IsAny<Page>(), It.IsAny<QueryCollection>(), It.IsAny<FormAnswers>()), Times.Once);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCall_IncomingDataHelper_AndSaveData_WhenFormHasIncomingGetValues() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(string.Empty);
            _mockIncomingDataHelper.Setup(_ => _.AddIncomingFormDataValues(It.IsAny<Page>(), It.IsAny<QueryCollection>(), It.IsAny<FormAnswers>()))
                .Returns(new Dictionary<string, dynamic> { { "test", "testdata" } });

            var element = new ElementBuilder()
                .WithType(EElementType.H1)
                .WithQuestionId("test-id")
                .WithPropertyText("test-text")
                .Build();

            var incomingGetValue = new IncomingValuesBuilder()
                .WithHttpActionType(EHttpActionType.Get)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithIncomingValue(incomingGetValue)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            // Act
            var result = await _service.ProcessPage("form", "page-one", "", new QueryCollection());

            // Assert
            Assert.IsType<ProcessPageEntity>(result);
            _mockIncomingDataHelper.Verify(_ => _.AddIncomingFormDataValues(It.IsAny<Page>(), It.IsAny<QueryCollection>(), It.IsAny<FormAnswers>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveNonQuestionAnswers(It.IsAny<Dictionary<string, object>>(), It.Is<string>(_ => _ == "form"), It.Is<string>(_ => _ == "page-one"), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessRequest_ShouldRedirect_WhenBookingService_ReturnsEntity_WithNoAppointments() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(string.Empty);
            _bookingService.Setup(_ => _.Get(It.IsAny<string>(), It.IsAny<Page>(), It.IsAny<string>()))
                .ReturnsAsync(new BookingProcessEntity { BookingHasNoAvailableAppointments = true });

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            // Act
            var result = await _service.ProcessPage("form", "page-one", "", new QueryCollection());

            // Assert
            Assert.IsType<ProcessPageEntity>(result);
            Assert.True(result.ShouldRedirect);
            Assert.Equal(BookingConstants.NO_APPOINTMENT_AVAILABLE, result.TargetPage);
            _bookingService.Verify(_ => _.Get(It.IsAny<string>(), It.IsAny<Page>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCall_BookingService_WhenPAge_ContainsBookingElement() {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _mockPageFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());
            _bookingService.Setup(_ => _.ProcessBooking(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProcessRequestEntity());

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() }
            };

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            var result = await _service.ProcessRequest("form", "page-one", viewModel, null, true);

            _bookingService.Verify(_ => _.ProcessBooking(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<Page>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCall_AddAnotherService_WhenPage_ContainsAddAnotherElement() {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>();
            var element = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithLabel("Person")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>(), null))
                .ReturnsAsync(schema);

            _mockAddAnotherService
                .Setup(_ => _.ProcessAddAnother(
                    It.IsAny<Dictionary<string, dynamic>>(),
                    It.IsAny<Page>(),
                    It.IsAny<FormSchema>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new ProcessRequestEntity());

            // Act
            await _service.ProcessRequest("form", "page-one", viewModel, null, true);

            // Assert
            _mockAddAnotherService.Verify(_ => _.ProcessAddAnother(
                It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<Page>(),
                It.IsAny<FormSchema>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetBehaviour_ShouldCallSession_And_DistributedCache() {
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
        public async Task FinalisePageJourney_ShouldThrow_ApplicationException_WhenNoSessionGuid() {
            // Arrange
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(string.Empty);

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.FinalisePageJourney("form", EBehaviourType.SubmitAndPay, schema));

            // Assert
            Assert.Equal("PageService::FinalisePageJourney: Session has expired", result.Message);
        }

        [Fact]
        public async Task FinalisePageJourney_ShouldThrow_ApplicationException_When_SessionData_IsNull() {
            // Arrange
            var guid = Guid.NewGuid();
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(guid.ToString());

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns((string)null);

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.FinalisePageJourney("form", EBehaviourType.SubmitAndPay, schema));

            // Assert
            Assert.Equal("PageService::FinalisePageJourney: Session data is null", result.Message);
        }

        [Fact]
        public async Task FinalisePageJourney_ShouldDeleteFileUpload_CacheEntries() {
            // Arrange
            var guid = Guid.NewGuid();
            var questionIDOne = "fileUploadone";
            var questionIDTwo = "fileUploadtwo";
            var fileOneKey = $"file-{questionIDOne}-12345";
            var fileTwoKey = $"file-{questionIDTwo}-12345";
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(guid.ToString());

            var cacheData = new FormAnswers {
                Path = "page-one",
                FormName = "form",
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = $"{questionIDOne}{FileUploadConstants.SUFFIX}",
                                Response = new List<FileUploadModel>
                                {
                                    new()
                                    {
                                        Key = fileOneKey
                                    }
                                }
                            },
                            new()
                            {
                                QuestionId = $"{questionIDTwo}{FileUploadConstants.SUFFIX}",
                                Response = new List<FileUploadModel>
                                {
                                    new()
                                    {
                                        Key = fileTwoKey
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId(questionIDOne)
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId(questionIDTwo)
                .Build();

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .WithElement(element2)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Act
            await _service.FinalisePageJourney("form", EBehaviourType.SubmitAndPay, schema);

            // Assert
            _fileStorageProvider.Verify(_ => _.Remove(It.Is<string>(x => x == fileOneKey)), Times.Once);
            _fileStorageProvider.Verify(_ => _.Remove(It.Is<string>(x => x == fileTwoKey)), Times.Once);
        }

        [Fact]
        public async Task FinalisePageJourney_ShouldDelete_All_Uploaded_Files_From_CacheEntries() {
            // Arrange
            var guid = Guid.NewGuid();
            var questionIDOne = "fileUploadone";
            var questionIDTwo = "fileUploadtwo";
            var fileOneKey = $"file-{questionIDOne}-123";
            var fileTwoKey = $"file-{questionIDOne}-456";
            var fileThreeKey = $"file-{questionIDOne}-789";
            var fileFourKey = $"file-{questionIDTwo}-123";
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(guid.ToString());

            var cacheData = new FormAnswers {
                Path = "page-one",
                FormName = "form",
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = $"{questionIDOne}{FileUploadConstants.SUFFIX}",
                                Response = new List<FileUploadModel>
                                {
                                    new()
                                    {
                                        Key = fileOneKey
                                    },
                                    new()
                                    {
                                        Key = fileTwoKey
                                    },
                                    new()
                                    {
                                        Key = fileThreeKey
                                    }
                                }
                            },
                            new()
                            {
                                QuestionId = $"{questionIDTwo}{FileUploadConstants.SUFFIX}",
                                Response = new List<FileUploadModel>
                                {
                                    new()
                                    {
                                        Key = fileFourKey
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId(questionIDOne)
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId(questionIDTwo)
                .Build();

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .WithElement(element2)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Act
            await _service.FinalisePageJourney("form", EBehaviourType.SubmitAndPay, schema);

            // Assert
            _fileStorageProvider.Verify(_ => _.Remove(It.Is<string>(x => x == fileOneKey)), Times.Once);
            _fileStorageProvider.Verify(_ => _.Remove(It.Is<string>(x => x == fileTwoKey)), Times.Once);
            _fileStorageProvider.Verify(_ => _.Remove(It.Is<string>(x => x == fileThreeKey)), Times.Once);
            _fileStorageProvider.Verify(_ => _.Remove(It.Is<string>(x => x == fileFourKey)), Times.Once);
        }

        [Fact]
        public async Task FinalisePageJourney_ShouldNotError_WhenFileUpload_DataIsNull() {
            // Arrange
            var guid = Guid.NewGuid();
            var questionIDOne = "fileUploadone";
            var questionIDTwo = "fileUploadtwo";
            var fileOneKey = $"file-{questionIDOne}-123";
            var fileTwoKey = $"file-{questionIDOne}-456";
            var fileThreeKey = $"file-{questionIDOne}-789";
            var fileFourKey = $"file-{questionIDTwo}-123";
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(guid.ToString());

            var cacheData = new FormAnswers {
                Path = "page-one",
                FormName = "form",
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = $"{questionIDOne}{FileUploadConstants.SUFFIX}",
                                Response = null,
                            },
                            new()
                            {
                                QuestionId = $"{questionIDTwo}{FileUploadConstants.SUFFIX}",
                                Response = null
                            }
                        }
                    }
                }
            };

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId(questionIDOne)
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId(questionIDTwo)
                .Build();

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .WithElement(element2)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Act
            await _service.FinalisePageJourney("form", EBehaviourType.SubmitAndPay, schema);

            // Assert
            _distributedCache.Verify(_ => _.Remove(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task FinalisePageJourney_Should_SetCache_WhenDocumentDownload_True() {
            // Arrange
            var guid = Guid.NewGuid();
            var questionIDOne = "questionOne";
            var questionIDTwo = "questionTwo";
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(guid.ToString());

            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId(questionIDOne)
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId(questionIDTwo)
                .Build();

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .WithElement(element2)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithDocumentDownload(true)
                .Build();

            // Act
            await _service.FinalisePageJourney("form", EBehaviourType.SubmitAndPay, schema);

            // Assert
            _distributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(x => x == $"document-{guid}"), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }


    }
}
