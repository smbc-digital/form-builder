using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.Services.FileUploadService;
using form_builder.Services.PageService.Entities;
using form_builder.Validators;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class FileUploadServiceTests
    {
        private readonly FileUploadService _service;
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new();
        private readonly Mock<IElementValidator> _testValidator = new();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new();
        private readonly Mock<IPageFactory> _mockPageFactory = new();
        private readonly Mock<IPageHelper> _mockPageHelper = new();

        private static readonly Element _element = new ElementBuilder()
            .WithType(EElementType.MultipleFileUpload)
            .WithQuestionId("fileUpload")
            .Build();

        private static readonly Page _page = new PageBuilder()
            .WithElement(_element)
            .WithValidatedModel(true)
            .WithPageSlug("page-one")
            .Build();

        private static readonly FormSchema _schema = new FormSchemaBuilder()
            .WithPage(_page)
            .WithBaseUrl("baseUrl")
            .Build();

        public FileUploadServiceTests()
        {
            _mockPageFactory
                .Setup(_ => _.Build(It.IsAny<Page>(),
                    It.IsAny<Dictionary<string, dynamic>>(),
                    It.IsAny<FormSchema>(),
                    It.IsAny<string>(),
                    It.IsAny<FormAnswers>(),
                    null))
                .ReturnsAsync(new FormBuilderViewModel());

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = true });

            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            _service = new FileUploadService(_mockDistributedCache.Object, _mockPageFactory.Object, _mockPageHelper.Object);
        }

        [Fact]
        public void AddFiles_ShouldReturnCorrectViewModel()
        {
            // Arrange
            var files = new List<DocumentModel>
            {
                new()
                {
                    Content = "content",
                    FileSize = 12,
                    FileName = "SMBC.png"
                },
                new()
                {
                    Content = "more content",
                    FileSize = 21,
                    FileName = "TEST.jpg"
                }
            };

            var expectedViewModel = new Dictionary<string, dynamic>
            {
                {
                    "fileUpload-fileupload", files
                }
            };

            var fileUpload = new List<CustomFormFile>
            {
                new("content", "fileUpload-fileupload", 12, "SMBC.png"),
                new("more content", "fileUpload-fileupload", 21, "TEST.jpg")
            };

            // Act
            var result = _service.AddFiles(new Dictionary<string, dynamic>(), fileUpload);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(expectedViewModel), JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task ProcessFile_RemoveFile_ShouldCallCacheProvider()
        {
            // Arrange
            var callbackCacheProvider = string.Empty;
            var cachedAnswers =
                "{\"FormName\":\"file-upload\",\"Path\":\"page-one\",\"CaseReference\":null,\"StartPageUrl\":null,\"FormData\":{},\"Pages\":[{\"PageSlug\":\"page-one\",\"Answers\":[{\"QuestionId\":\"fileUpload-fileupload\",\"Response\":[{\"Key\":\"file-fileUpload-fileupload-b3df0129-c527-4fb8-8cd6-e35e622116f6\",\"TrustedOriginalFileName\":\"SMBC.png\",\"UntrustedOriginalFileName\":\"SMBC.png\",\"Content\":null,\"FileSize\":26879,\"FileName\":null}]}]}]}";

            var viewModel = new Dictionary<string, dynamic>
            {
                {$"{FileUploadConstants.FILE_TO_DELETE}", "SMBC.png" }
            };

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("file-upload")
                .Build();

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(cachedAnswers);

            _mockDistributedCache.Setup(_ =>
                    _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            // Act
            await _service.ProcessFile(viewModel, It.IsAny<Page>(), formSchema, Guid.NewGuid().ToString(), "page-one", null, true);

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task ProcessFile_RemoveFile_ShouldRemoveFileFromResponse()
        {
            // Arrange
            var callbackCacheProvider = string.Empty;
            var cachedAnswers =
                "{\"FormName\":\"file-upload\",\"Path\":\"page-one\",\"CaseReference\":null,\"StartPageUrl\":null,\"FormData\":{},\"AdditionalFormData\":{},\"Pages\":[{\"PageSlug\":\"page-one\",\"Answers\":[{\"QuestionId\":\"fileUpload-fileupload\",\"Response\":[{\"Key\":\"file-fileUpload-fileupload-b3df0129-c527-4fb8-8cd6-e35e622116f6\",\"TrustedOriginalFileName\":\"SMBC.png\",\"UntrustedOriginalFileName\":\"SMBC.png\",\"Content\":null,\"FileSize\":26879,\"FileName\":null}]}]}]}";

            var updatedAnswers =
                "{\"FormName\":\"file-upload\",\"Path\":\"page-one\",\"CaseReference\":null,\"StartPageUrl\":null,\"FormData\":{},\"AdditionalFormData\":{},\"Pages\":[{\"PageSlug\":\"page-one\",\"Answers\":[{\"QuestionId\":\"fileUpload-fileupload\",\"Response\":[]}]}]}";

            var viewModel = new Dictionary<string, dynamic>
            {
                {$"{FileUploadConstants.FILE_TO_DELETE}", "SMBC.png" }
            };

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("file-upload")
                .Build();

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(cachedAnswers);

            _mockDistributedCache.Setup(_ =>
                    _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            // Act
            var result = await _service.ProcessFile(viewModel, It.IsAny<Page>(), formSchema, Guid.NewGuid().ToString(), "page-one", null, true);

            // Assert
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), updatedAnswers, CancellationToken.None), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessFile_ProcessSelectedFiles_ShouldCallPageFactory_IfCurrentPageInvalid()
        {
            // Arrange
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
                .WithBaseUrl("baseUrl")
                .Build();

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = false });

            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            page.Validate(new Dictionary<string, dynamic>(), _validators.Object, new FormSchema());

            // Act
            var result = await _service.ProcessFile(new Dictionary<string, dynamic>(), page, schema,
                new Guid().ToString(), It.IsAny<string>(), null, true);

            // Assert
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), null), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessFile_ProcessSelectedFiles_ShouldReturnProcessEntity_WithOnlyCurrentPage_IfSubmittingWithNoFilesInViewModel()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "Submit", "Submit"
                }
            };

            // Act
            var result = await _service.ProcessFile(viewModel, _page, _schema,
                new Guid().ToString(), It.IsAny<string>(), null, true);

            // Assert
            Assert.IsType<ProcessRequestEntity>(result);
            Assert.Equal("page-one", result.Page.PageSlug);
            Assert.False(result.RedirectToAction);
            Assert.Null(result.RouteValues);
        }

        [Fact]
        public async Task ProcessFile_ProcessSelectedFiles_ShouldReturnProcessEntity_WithRedirect_IfNotSubmittingWithNoFilesInViewModel()
        {
            // Arrange
            var expectedRouteValues = new
            {
                form = "baseUrl",
                path = "path"
            };

            // Act
            var result = await _service.ProcessFile(new Dictionary<string, dynamic>(), _page, _schema,
                new Guid().ToString(), "path", null, true);

            // Assert
            Assert.IsType<ProcessRequestEntity>(result);
            Assert.True(result.RedirectToAction);
            Assert.Equal("Index", result.RedirectAction);
            Assert.Equal(JsonConvert.SerializeObject(expectedRouteValues), JsonConvert.SerializeObject(result.RouteValues));
            Assert.Null(result.Page);
        }

        [Fact]
        public async Task ProcessFile_ProcessSelectedFiles_ShouldCallPageHelper_AndReturnCorrectProcessRequestEntity_IfFilesExistAndIsSubmitting()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "Submit", "Submit"
                }
            };

            var fileUpload = new List<CustomFormFile>
            {
                new("content", "fileUpload-fileupload", 12, "SMBC.png"),
                new("more content", "fileUpload-fileupload", 21, "TEST.jpg")
            };

            // Act
            var result = await _service.ProcessFile(viewModel, _page, _schema, new Guid().ToString(),
                It.IsAny<string>(), fileUpload, true);

            // Assert
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CustomFormFile>>(), true, true, false), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
            Assert.Equal("page-one", result.Page.PageSlug);
            Assert.False(result.RedirectToAction);
            Assert.Equal(string.Empty, result.RedirectAction);
            Assert.Null(result.RouteValues);
        }

        [Fact]
        public async Task ProcessFile_ProcessSelectedFiles_ShouldCallPageHelper_AndReturnCorrectProcessRequestEntity_IfFilesExistAndIsNotSubmitting()
        {
            // Arrange
            var expectedRouteValues = new
            {
                form = "baseUrl",
                path = "path"
            };

            var fileUpload = new List<CustomFormFile>
            {
                new("content", "fileUpload-fileupload", 12, "SMBC.png"),
                new("more content", "fileUpload-fileupload", 21, "TEST.jpg")
            };

            // Act
            var result = await _service.ProcessFile(new Dictionary<string, dynamic>(), _page, _schema, new Guid().ToString(),
                "path", fileUpload, true);

            // Assert
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CustomFormFile>>(), true, true, false), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
            Assert.True(result.RedirectToAction);
            Assert.Equal("Index", result.RedirectAction);
            Assert.Equal(JsonConvert.SerializeObject(expectedRouteValues), JsonConvert.SerializeObject(result.RouteValues));
            Assert.Null(result.Page);
        }

        [Fact]
        public async Task ProcessFile_ProcessSelectedFiles_ShouldCallPageHelper_IfSubmitWithFilesButModelStateNotValid()
        {
            // Arrange
            var fileUpload = new List<CustomFormFile>
            {
                new("content", "fileUpload-fileupload", 12, "SMBC.png"),
                new("more content", "fileUpload-fileupload", 21, "TEST.jpg")
            };

            // Act
            await _service.ProcessFile(new Dictionary<string, dynamic>(), _page, _schema, new Guid().ToString(),
                "path", fileUpload, false);

            // Assert
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CustomFormFile>>(), true, true, false), Times.Once);
        }

        [Fact]
        public async Task ProcessFile_ProcessSelectedFiles_ShouldNotCallPageHelper_IfSubmitWithoutFilesButModelStateNotValid()
        {
            // Arrange

            // Act
            await _service.ProcessFile(new Dictionary<string, dynamic>(), _page, _schema, new Guid().ToString(),
                "path", null, false);

            // Assert
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CustomFormFile>>(), It.IsAny<bool>(), true, false), Times.Never);
        }

        [Fact]
        public async Task ProcessFile_ProcessSelectedFiles_ShouldReturnCorrectProcessRequestEntity_IfSubmittingWithModelStateInvalid()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "Submit", "Submit"
                }
            };

            var fileUpload = new List<CustomFormFile>
            {
                new("content", "fileUpload-fileupload", 12, "SMBC.png"),
                new("more content", "fileUpload-fileupload", 21, "TEST.jpg")
            };

            // Act
            var result = await _service.ProcessFile(viewModel, _page, _schema, new Guid().ToString(),
                "path", fileUpload, false);

            // Assert
            Assert.IsType<ProcessRequestEntity>(result);
            Assert.False(result.RedirectToAction);
            Assert.NotNull(result.Page);
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), fileUpload, true, true, false), Times.Once);
        }

        [Fact]
        public async Task ProcessFile_ProcessSelectedFiles_ShouldCallPageFactory_IfModelStateInvalid()
        {
            // Arrange
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
                .WithBaseUrl("baseUrl")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "Submit", "Submit"
                }
            };

            // Act
            var result = await _service.ProcessFile(viewModel, page, schema,
                new Guid().ToString(), It.IsAny<string>(), null, false);

            // Assert
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), null), Times.Once);
            Assert.IsType<ProcessRequestEntity>(result);
            Assert.True(result.UseGeneratedViewModel);
        }

        [Fact]
        public async Task ProcessFile_ProcessSelectedFiles_ShouldSave_WhenMultipleFileUpload_IsOptional_WithNoFiles_OnSubmit()
        {




            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileUpload")
                .WithOptional(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithBaseUrl("baseUrl")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "Submit", "Submit"
                }
            };

            // Act
            var result = await _service.ProcessFile(viewModel, page, schema, new Guid().ToString(),
                "path", null, true);

            // Assert
            Assert.IsType<ProcessRequestEntity>(result);
            Assert.False(result.RedirectToAction);
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), null, true, true, false), Times.Once);
        }
    }
}
