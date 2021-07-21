using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.ContentFactory.SuccessPageFactory;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Transforms.PaymentConfiguration;
using form_builder.Services.FileUploadService;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.PageService.Entities;
using form_builder.Services.PayService;
using form_builder.Services.PreviewService;
using form_builder.Validators;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class PreviewServiceTests
    {
        private readonly PreviewService _service;
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new();
        private readonly Mock<IElementValidator> _testValidator = new();
        private readonly Mock<IFileUploadService> _fileUploadService = new();
        private readonly Mock<IDistributedCacheWrapper> _distributedCache = new();
        private readonly Mock<ISchemaFactory> _schemaFactory = new();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistributedCacheExpirationConfiguration = new();
        private readonly Mock<IOptions<PreviewModeConfiguration>> _mockPreviewModeConfiguration = new();
        private readonly Mock<IOptions<ApplicationVersionConfiguration>> _mockApplicationVersionConfiguration = new();
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
        private readonly Mock<IPageFactory> _mockPageFactory = new();

        public PreviewServiceTests()
        {
            _mockDistributedCacheExpirationConfiguration
                .Setup(_ => _.Value)
                .Returns(new DistributedCacheExpirationConfiguration{ FormJson = 10 });

            _mockApplicationVersionConfiguration
                .Setup(_ => _.Value)
                .Returns(new ApplicationVersionConfiguration{ Version = "v2" });

            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration{ IsEnabled = true });

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = false });

            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            _service = new PreviewService(_validators.Object,
                _fileUploadService.Object,
                _distributedCache.Object,
                _schemaFactory.Object,
                _mockDistributedCacheExpirationConfiguration.Object,
                _mockPreviewModeConfiguration.Object,
                _mockApplicationVersionConfiguration.Object,
                _mockHttpContextAccessor.Object,
                _mockPageFactory.Object
            );
        }

        [Fact]
        public async Task GetPreviewPage_ShouldThrow_Exception_When_PreviewMode_IsDisabled()
        {
            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration{ IsEnabled = false });

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.GetPreviewPage());

            Assert.Equal("PreviewService: Request to access preview service recieved but preview service is disabled in current enviroment", result.Message);
        }

        [Fact]
        public async Task GetPreviewPage_ShouldReturn_PreviewPage()
        {
            _mockPageFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            var result = await _service.GetPreviewPage();

            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
            Assert.IsType<FormBuilderViewModel>(result);
        }

        [Fact]
        public void ExitPreviewMode_ShouldThrow_Exception_When_PreviewMode_IsDisabled()
        {
             _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration{ IsEnabled = false });

            var result = Assert.Throws<ApplicationException>(() => _service.ExitPreviewMode());

            Assert.Equal("PreviewService: Request to exit preview mode recieved but preview service is disabled in current enviroment", result.Message);
        }

        [Fact(Skip="wip")]
        public void ExitPreviewMode_Should_Clear_PreviewSchema_FromCache_AndDelete_Cookie()
        {
            var cookieKey = "test-schamea-key-123";

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();

            var mockContext = new Mock<HttpContext>();
            var mockHttpResponse = new Mock<HttpResponse>();
            var requestFeature = new HttpRequestFeature();
            var featureCollection = new FeatureCollection();
            requestFeature.Headers = new HeaderDictionary();
            requestFeature.Headers.Add("cookie", new StringValues(CookieConstants.PREVIEW_MODE + "=" + cookieKey));
            featureCollection.Set<IHttpRequestFeature>(requestFeature);
            var cookiesFeatureRequest = new RequestCookiesFeature(featureCollection);
            var cookiesFeatureResponse = new ResponseCookiesFeature(featureCollection);

            mockHttpResponse.Setup(_ => _.Cookies)
                .Returns(cookiesFeatureResponse.Cookies);

            mockContext.Setup(_ => _.Response)
                .Returns(mockHttpResponse.Object);

            mockContext.Setup(_ => _.Request.Cookies)
                .Returns(cookiesFeatureRequest.Cookies);

            _mockHttpContextAccessor.Setup(_ => _.HttpContext)
                .Returns(mockContext.Object);

            _mockPageFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            _service.ExitPreviewMode();

            _distributedCache.Verify(_ => _.Remove(It.Is<string>(_ => _.EndsWith(cookieKey))), Times.Never);
        }

        [Fact]
        public async Task VerifyPreviewRequest_ShouldThrow_Exception_When_PreviewMode_IsDisabled()
        {
            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration{ IsEnabled = false });

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.VerifyPreviewRequest(new List<CustomFormFile>()));

            Assert.Equal("PreviewService: Request to upload from in preview service recieved but preview service is disabled in current enviroment", result.Message);
        }

        [Fact]
        public async Task VerifyPreviewRequest_ShouldRetuenView_WhenRequest_IsNotValid()
        {
            var result = await _service.VerifyPreviewRequest(new List<CustomFormFile>());
            
            Assert.IsType<ProcessPreviewRequestEntity>(result);
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
            _testValidator.Verify(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()), Times.Once);
        }

        [Fact]
        public async Task VerifyPreviewRequest_ShouldReturnErrorPage_When_Uploaded_Schama_Is_Not_Valid()
        {
            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = true });

            var model = new List<CustomFormFile>
            {
                new CustomFormFile("", "", 0, "")
            };

            _schemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ThrowsAsync(new ApplicationException());

            _fileUploadService.Setup(_ => _.AddFiles(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .Returns(new Dictionary<string, dynamic>{ { "file", new List<DocumentModel> { new DocumentModel{ Content = "" }} } });

            var result = await _service.VerifyPreviewRequest(model);
            
            var entity = Assert.IsType<ProcessPreviewRequestEntity>(result);
            Assert.True(entity.UseGeneratedViewModel);
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
            _testValidator.Verify(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()), Times.Once);
            _distributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(_ => _.StartsWith($"form-json-v2-{PreviewConstants.PREVIEW_MODE_PREFIX}")), It.IsAny<string>(), It.Is<int>(_ => _.Equals(10)), It.IsAny<CancellationToken>()), Times.Once);
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(_ => _.StartsWith($"form-json-v2-{PreviewConstants.PREVIEW_MODE_PREFIX}"))), Times.Once);
        }

        [Fact(Skip="wip")]
        public async Task VerifyPreviewRequest_Should_Return_ProcessPreviewRequestEntity_OnSuccessfuly_PreviewRequest()
        {
            var mockContext = new Mock<HttpContext>();
            var mockHttpResponse = new Mock<HttpResponse>();
            
            var requestFeature = new HttpRequestFeature();
            var featureCollection = new FeatureCollection();
            requestFeature.Headers = new HeaderDictionary();
            var cookiesFeatureResponse = new ResponseCookiesFeature(featureCollection);

            mockContext.SetupGet(x => x.Response).Returns(mockHttpResponse.Object);
            mockContext.SetupGet(x => x.Response.Cookies).Returns(cookiesFeatureResponse.Cookies);

            mockContext.Setup(_ => _.Response)
                .Returns(mockHttpResponse.Object);

            _mockHttpContextAccessor.Setup(_ => _.HttpContext)
                .Returns(mockContext.Object);

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = true });

            var model = new List<CustomFormFile>
            {
                new CustomFormFile("", "", 0, "")
            };

            _schemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(new FormSchema{ Pages = new List<Page>() });

            _fileUploadService.Setup(_ => _.AddFiles(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .Returns(new Dictionary<string, dynamic>{ { "file", new List<DocumentModel> { new DocumentModel{ Content = "" }} } });

            var result = await _service.VerifyPreviewRequest(model);
            
            var entity = Assert.IsType<ProcessPreviewRequestEntity>(result);
            Assert.StartsWith(PreviewConstants.PREVIEW_MODE_PREFIX, entity.PreviewFormKey);
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
            _testValidator.Verify(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()), Times.Once);
            _distributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(_ => _.StartsWith($"form-json-v2-{PreviewConstants.PREVIEW_MODE_PREFIX}")), It.IsAny<string>(), It.Is<int>(_ => _.Equals(10)), It.IsAny<CancellationToken>()), Times.Exactly(3));
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(_ => _.StartsWith($"form-json-v2-{PreviewConstants.PREVIEW_MODE_PREFIX}"))), Times.Never);
        }
    }
}