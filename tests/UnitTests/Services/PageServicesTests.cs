using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Helpers.IncomingDataHelper;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.Services.AddressService;
using form_builder.Services.FileUploadService;
using form_builder.Services.MappingService;
using form_builder.Services.OrganisationService;
using form_builder.Services.PageService;
using form_builder.Services.PageService.Entities;
using form_builder.Services.PayService;
using form_builder.Services.StreetService;
using form_builder.Validators;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class PageServicesTests
    {
        private readonly PageService _service;
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private readonly Mock<IElementValidator> _validator = new Mock<IElementValidator>();
        private readonly Mock<IPageHelper> _mockPageHelper = new Mock<IPageHelper>();
        private readonly Mock<ISessionHelper> _sessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IStreetService> _streetService = new Mock<IStreetService>();
        private readonly Mock<IAddressService> _addressService = new Mock<IAddressService>();
        private readonly Mock<IFileUploadService> _fileUploadService = new Mock<IFileUploadService>();
        private readonly Mock<IOrganisationService> _organisationService = new Mock<IOrganisationService>();
        private readonly Mock<IDistributedCacheWrapper> _distributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<ISchemaFactory> _mockSchemaFactory = new Mock<ISchemaFactory>();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistributedCacheExpirationConfiguration = new Mock<IOptions<DistributedCacheExpirationConfiguration>>();
        private readonly Mock<IWebHostEnvironment> _mockEnvironment = new Mock<IWebHostEnvironment>();
        private readonly Mock<IPayService> _payService = new Mock<IPayService>();
        private readonly Mock<IMappingService> _mappingService = new Mock<IMappingService>();
        private readonly Mock<IPageFactory> _mockPageFactory = new Mock<IPageFactory>();
        private readonly Mock<IIncomingDataHelper> _mockIncomingDataHelper = new Mock<IIncomingDataHelper>();
        private readonly Mock<ISuccessPageFactory> _mockSuccessPageFactory = new Mock<ISuccessPageFactory>();

        public PageServicesTests()
        {
            _validator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Returns(new ValidationResult { IsValid = false });
            var elementValidatorItems = new List<IElementValidator> { _validator.Object };
            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            _mockPageHelper
                .Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                        .ReturnsAsync(new FormBuilderViewModel());

            _mockPageHelper
                .Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), null))
                        .ReturnsAsync(new FormBuilderViewModel());

            _mockEnvironment.Setup(_ => _.EnvironmentName)
                .Returns("local");

            var cacheData = new FormAnswers
            {
                Path = "page-one",
                FormName = "form"
            };

            _distributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

            _mockDistributedCacheExpirationConfiguration.Setup(_ => _.Value).Returns(new DistributedCacheExpirationConfiguration
            {
                FormJson = 1
            });

            _service = new PageService(_validators.Object, _mockPageHelper.Object, _sessionHelper.Object, _addressService.Object, _fileUploadService.Object, _streetService.Object, _organisationService.Object, 
            _distributedCache.Object, _mockDistributedCacheExpirationConfiguration.Object, _mockEnvironment.Object, _mockSuccessPageFactory.Object, _mockPageFactory.Object, _mockSchemaFactory.Object, _mappingService.Object, _payService.Object, _mockIncomingDataHelper.Object);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCall_Schema_And_Session_Service()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");
            _mockPageFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, dynamic>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            var result = await _service.ProcessRequest("form", "page-one", viewModel, null, true);

            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
            _mockSchemaFactory.Verify(_ => _.Build(It.IsAny<string>()), Times.Once);
            _sessionHelper.Verify(_ => _.GetSessionGuid(), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessPage_ShouldThrowException_IfFormIsNotAvailable()
        {
            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("local", false)
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPage("form", "page-one", "", new QueryCollection()));
        }

        [Fact]
        public async Task ProcessRequest_ShouldThrowException_IfFormIsNotAvailable()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("1234567");

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("local", false)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessRequest("form", "page-one", viewModel, null, true));
        }

        [Fact]
        public async Task ProcessRequest_ShouldCallAddressService_WhenAddressElement()
        {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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
        public async Task ProcessRequest_ApplicationShould_ThrowApplicationException_WhenGenerateHtml_ThrowsException()
        {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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
        public async Task ProcessPage_ShouldCallSchemaFactory_ToGetFormSchema()
        {
            // Act
            await Assert.ThrowsAsync<NullReferenceException>(() => _service.ProcessPage("form", "page-one", "", new QueryCollection()));

            // Assert
            _mockSchemaFactory.Verify(_ => _.Build(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPage_ShouldGenerateGuidWhenGuidIsEmpty()
        {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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
        public async Task ProcessPage_Application_ShouldThrowNullException_WhenPageIsNotWithin_FormSchema()
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns((Page) null);

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPage("form", requestPath, "", new QueryCollection()));
            Assert.Equal($"Requested path '{requestPath}' object could not be found.", result.Message);
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

            _mockSchemaFactory
                .Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            var result = await _service.ProcessPage("form", "page-one", LookUpConstants.Manual, new QueryCollection());

            Assert.Equal("Index", result.ViewName);
            Assert.False(result.ShouldRedirect);
        }

        [Fact]
        public async Task ProcessPage_ShouldCallDistributedCache_ToDeleteSessionData_WhenNavigating_ToDifferentForm()
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
                .WithFirstPageSlug("page-one")
                .WithBaseUrl("new-form")
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            await _service.ProcessPage("new-form", "page-one", "", new QueryCollection());

            _distributedCache.Verify(_ => _.Remove(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPage_ShouldNotCallDistributedCache_ToDeleteSessionData_WhenNavigating_ToDifferentForm()
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
                .WithFirstPageSlug("page-one")
                .WithBaseUrl("new-form")
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            await _service.ProcessPage("new-form", "page-one", "", new QueryCollection());

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
        public async Task ProcessRequest_ShouldCallProcessOrganisation_WhenOrganisationElement()
        {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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
        public async Task ProcessPage_ShouldGetTheRightStartPageUrl()
        {
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

            var viewModel = new FormBuilderViewModel
            {
                StartPageUrl = "https://www.test.com/textbox/page-one"
            };

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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
        public async Task ProcessRequest_ShouldGetTheRightStartPageUrl()
        {
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

            var viewModel = new FormBuilderViewModel
            {
                StartPageUrl = "https://www.test.com/textarea/first-page"
            };

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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
        public async Task FinalisePageJourney_ShouldDeleteFileUpload_CacheEntries()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var questionIDOne = "fileUploadone";
            var questionIDTwo = "fileUploadtwo";
            var fileOneKey = $"file-{questionIDOne}-12345";
            var fileTwoKey = $"file-{questionIDTwo}-12345";
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(guid.ToString());

            var cacheData = new FormAnswers
            {
                Path = "page-one",
                FormName = "form",
                Pages = new List<PageAnswers> 
                {
                    new PageAnswers 
                    {
                        Answers = new List<Answers> 
                        {
                            new Answers 
                            {
                                QuestionId = $"{questionIDOne}{FileUploadConstants.SUFFIX}",
                                Response = new List<FileUploadModel>
                                {
                                    new FileUploadModel 
                                    {
                                        Key = fileOneKey
                                    }
                                }
                            },
                            new Answers 
                            {
                                QuestionId = $"{questionIDTwo}{FileUploadConstants.SUFFIX}",
                                Response = new List<FileUploadModel>
                                {
                                    new FileUploadModel 
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
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(x => x == fileOneKey)), Times.Once);
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(x => x == fileTwoKey)), Times.Once);
        }


        [Fact]
        public async Task FinalisePageJourney_ShouldDelete_All_Uploaded_Files_From_CacheEntries()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var questionIDOne = "fileUploadone";
            var questionIDTwo = "fileUploadtwo";
            var fileOneKey = $"file-{questionIDOne}-123";
            var fileTwoKey = $"file-{questionIDOne}-456";
            var fileThreeKey = $"file-{questionIDOne}-789";
            var fileFourKey = $"file-{questionIDTwo}-123";
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(guid.ToString());

            var cacheData = new FormAnswers
            {
                Path = "page-one",
                FormName = "form",
                Pages = new List<PageAnswers> 
                {
                    new PageAnswers 
                    {
                        Answers = new List<Answers> 
                        {
                            new Answers 
                            {
                                QuestionId = $"{questionIDOne}{FileUploadConstants.SUFFIX}",
                                Response = new List<FileUploadModel>
                                {
                                    new FileUploadModel 
                                    {
                                        Key = fileOneKey
                                    },
                                    new FileUploadModel 
                                    {
                                        Key = fileTwoKey
                                    },
                                    new FileUploadModel 
                                    {
                                        Key = fileThreeKey
                                    }
                                }
                            },
                            new Answers 
                            {
                                QuestionId = $"{questionIDTwo}{FileUploadConstants.SUFFIX}",
                                Response = new List<FileUploadModel>
                                {
                                    new FileUploadModel 
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
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(x => x == fileOneKey)), Times.Once);
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(x => x == fileTwoKey)), Times.Once);
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(x => x == fileThreeKey)), Times.Once);
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(x => x == fileFourKey)), Times.Once);
        }

        [Fact]
        public async Task FinalisePageJourney_ShouldNotError_WhenFileUPload_DataIsNull()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var questionIDOne = "fileUploadone";
            var questionIDTwo = "fileUploadtwo";
            var fileOneKey = $"file-{questionIDOne}-123";
            var fileTwoKey = $"file-{questionIDOne}-456";
            var fileThreeKey = $"file-{questionIDOne}-789";
            var fileFourKey = $"file-{questionIDTwo}-123";
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(guid.ToString());

            var cacheData = new FormAnswers
            {
                Path = "page-one",
                FormName = "form",
                Pages = new List<PageAnswers> 
                {
                    new PageAnswers 
                    {
                        Answers = new List<Answers> 
                        {
                            new Answers 
                            {
                                QuestionId = $"{questionIDOne}{FileUploadConstants.SUFFIX}",
                                Response = null,
                            },
                            new Answers 
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
        public async Task FinalisePageJourney_Should_SetCache_WhenDocumentDownload_True()
        {
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

        [Fact]
        public async Task ProcessRequest_ShouldNot_CallPageHelper_WhenPageContains_NoInboundValues()
        {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            await _service.ProcessRequest("form", "page-one", new Dictionary<string, dynamic>(), null, true);

            _mockIncomingDataHelper.Verify(_ => _.AddIncomingFormDataValues(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessRequest_Should_CallPageHelper_WhenPageContains_InboundValues()
        {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            await _service.ProcessRequest("form", "page-one", new Dictionary<string, dynamic>(), It.IsAny<IEnumerable<CustomFormFile>>(), true);

            _mockIncomingDataHelper.Verify(_ => _.AddIncomingFormDataValues(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessRequest_ShouldCallFileUploadService_WhenMultipleFileUploadElement()
        {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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
        public async Task ProcessRequest_ShouldNotCallFileUploadService_WhenNoMultipleFileUploadElement()
        {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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
        public async Task ProcessRequest_ShouldCall_IncomingDataHelper_WhenFormHasIncomingGetValues()
        {
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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
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
        public async Task ProcessRequest_ShouldCall_IncomingDataHelper_AndSaveData_WhenFormHasIncomingGetValues()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns(string.Empty);
            _mockIncomingDataHelper.Setup(_ => _.AddIncomingFormDataValues(It.IsAny<Page>(), It.IsAny<QueryCollection>(), It.IsAny<FormAnswers>()))
                .Returns(new Dictionary<string, dynamic>{ { "test", "testdata"} });

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

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(page);

            // Act
            var result = await _service.ProcessPage("form", "page-one", "", new QueryCollection());

            // Assert
            Assert.IsType<ProcessPageEntity>(result);
            _mockIncomingDataHelper.Verify(_ => _.AddIncomingFormDataValues(It.IsAny<Page>(), It.IsAny<QueryCollection>(), It.IsAny<FormAnswers>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveNonQuestionAnswers(It.IsAny<Dictionary<string, object>>(), It.Is<string>(_ => _ == "form"), It.Is<string>(_ => _ == "page-one"),It.IsAny<string>()), Times.Once);
        }
    }
}
