using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.Factories.Schema;
using form_builder.Helpers.Cookie;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.Services.FileUploadService;
using form_builder.Services.PageService.Entities;
using form_builder.Services.PreviewService;
using form_builder.Validators;
using form_builder.ViewModels;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
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
        private readonly Mock<IOptions<PreviewModeConfiguration>> _mockPreviewModeConfiguration = new();
        private readonly Mock<ICookieHelper> _mockCookieHelper = new();
        private readonly Mock<IPageFactory> _mockPageFactory = new();

        public PreviewServiceTests()
        {
            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration { IsEnabled = true });

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = false });

            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            _service = new PreviewService(_validators.Object,
                _fileUploadService.Object,
                _distributedCache.Object,
                _schemaFactory.Object,
                _mockPreviewModeConfiguration.Object,
                _mockCookieHelper.Object,
                _mockPageFactory.Object
            );
        }

        [Fact]
        public async Task GetPreviewPage_ShouldThrow_Exception_When_PreviewMode_IsDisabled()
        {
            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration { IsEnabled = false });

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
               .Returns(new PreviewModeConfiguration { IsEnabled = false });

            var result = Assert.Throws<ApplicationException>(() => _service.ExitPreviewMode());

            Assert.Equal("PreviewService: Request to exit preview mode recieved but preview service is disabled in current enviroment", result.Message);
        }

        [Fact]
        public void ExitPreviewMode_Should_Clear_PreviewSchema_FromCache_AndDelete_Cookie()
        {
            var cookieKey = "test-schamea-key-123";

            _mockCookieHelper.Setup(_ => _.GetCookie(It.IsAny<string>()))
                .Returns(cookieKey);

            _mockPageFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            _service.ExitPreviewMode();

            _distributedCache.Verify(_ => _.Remove(It.Is<string>(_ => _.EndsWith(cookieKey))), Times.Once);
            _mockCookieHelper.Verify(_ => _.DeleteCookie(It.Is<string>(x => x.Equals(cookieKey))), Times.Once);
            _mockCookieHelper.Verify(_ => _.GetCookie(It.Is<string>(x => x.Equals(CookieConstants.PREVIEW_MODE))), Times.Once);
        }

        [Fact]
        public async Task VerifyPreviewRequest_ShouldThrow_Exception_When_PreviewMode_IsDisabled()
        {
            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration { IsEnabled = false });

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
                .Returns(new Dictionary<string, dynamic> { { "file", new List<DocumentModel> { new DocumentModel { Content = "" } } } });

            var result = await _service.VerifyPreviewRequest(model);

            var entity = Assert.IsType<ProcessPreviewRequestEntity>(result);
            Assert.True(entity.UseGeneratedViewModel);
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
            _testValidator.Verify(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()), Times.Once);
            _distributedCache.Verify(_ => _.SetAsync(It.Is<string>(_ => _.StartsWith($"form-json-{PreviewConstants.PREVIEW_MODE_PREFIX}")), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(_ => _.StartsWith($"form-json-{PreviewConstants.PREVIEW_MODE_PREFIX}"))), Times.Once);
        }

        [Fact]
        public async Task VerifyPreviewRequest_ShouldReturnErrorPage_When_Uploaded_Schama_Is_Not_Valid_AndAdd_MesseageWhen_ExceptionType_Is_From_Newtonsoft()
        {
            Page callbackPage = new();
            _mockPageFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel())
                .Callback<Page, Dictionary<string, dynamic>, FormSchema, string, FormAnswers, object>((a, b, c, d, e, f) => callbackPage = a);

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = true });

            var model = new List<CustomFormFile>
            {
                new CustomFormFile("", "", 0, "")
            };

            _schemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ThrowsAsync(new ApplicationException { Source = LibConstants.NEWTONSOFT_LIBRARY_NAME });

            _fileUploadService.Setup(_ => _.AddFiles(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .Returns(new Dictionary<string, dynamic> { { "file", new List<DocumentModel> { new DocumentModel { Content = "" } } } });

            var result = await _service.VerifyPreviewRequest(model);

            var entity = Assert.IsType<ProcessPreviewRequestEntity>(result);
            Assert.True(entity.UseGeneratedViewModel);
            Assert.Equal(3, callbackPage.Elements.Count);
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
            _testValidator.Verify(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()), Times.Once);
            _distributedCache.Verify(_ => _.SetAsync(It.Is<string>(_ => _.StartsWith($"form-json-{PreviewConstants.PREVIEW_MODE_PREFIX}")), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(_ => _.StartsWith($"form-json-{PreviewConstants.PREVIEW_MODE_PREFIX}"))), Times.Once);
        }

        [Fact]
        public async Task VerifyPreviewRequest_Should_Return_ProcessPreviewRequestEntity_OnSuccessfuly_PreviewRequest()
        {
            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = true });

            var model = new List<CustomFormFile>
            {
                new CustomFormFile("", "", 0, "")
            };

            _schemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync(new FormSchema { Pages = new List<Page>() });

            _fileUploadService.Setup(_ => _.AddFiles(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<IEnumerable<CustomFormFile>>()))
                .Returns(new Dictionary<string, dynamic> { { "file", new List<DocumentModel> { new DocumentModel { Content = "" } } } });

            var result = await _service.VerifyPreviewRequest(model);

            var entity = Assert.IsType<ProcessPreviewRequestEntity>(result);
            Assert.StartsWith(PreviewConstants.PREVIEW_MODE_PREFIX, entity.PreviewFormKey);
            _testValidator.Verify(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()), Times.Once);
            _mockCookieHelper.Verify(_ => _.AddCookie(It.Is<string>(x => x.Equals(CookieConstants.PREVIEW_MODE)), It.Is<string>(y => y.StartsWith(PreviewConstants.PREVIEW_MODE_PREFIX))), Times.Once);
            _distributedCache.Verify(_ => _.SetAsync(It.Is<string>(_ => _.StartsWith($"form-json-{PreviewConstants.PREVIEW_MODE_PREFIX}")), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _distributedCache.Verify(_ => _.SetStringAbsoluteAsync(It.Is<string>(_ => _.StartsWith($"form-json-{PreviewConstants.PREVIEW_MODE_PREFIX}")), It.IsAny<string>(), It.Is<int>(_ => _.Equals(30)), It.IsAny<CancellationToken>()), Times.Once);
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Never);
            _distributedCache.Verify(_ => _.Remove(It.Is<string>(_ => _.StartsWith($"form-json-{PreviewConstants.PREVIEW_MODE_PREFIX}"))), Times.Never);
        }
    }
}