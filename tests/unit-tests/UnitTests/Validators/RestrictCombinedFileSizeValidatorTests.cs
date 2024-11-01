using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Validators;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictCombinedFileSizeValidatorTests
    {
        private readonly RestrictCombinedFileSizeValidator _validator;
        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCacheWrapper = new Mock<IDistributedCacheWrapper>();

        public RestrictCombinedFileSizeValidatorTests()
        {
            _validator = new RestrictCombinedFileSizeValidator(_mockSessionHelper.Object, _mockDistributedCacheWrapper.Object);
        }

        [Theory]
        [InlineData(EElementType.Textarea)]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Radio)]
        public void Validate_ShouldReturn_True_ValidationResult_WhenNot_MultipleFileUpload_ElementType(EElementType type)
        {
            var element = new ElementBuilder()
                .WithType(type)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenQuestion_NowWithinViewModel()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileuploadquestion")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenValueIn_ViewModel_IsNull()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileuploadquestion")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {$"fileuploadquestion{FileUploadConstants.SUFFIX}", null }
            };
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenSingle_TotalFiles_AreBelow_MaxLimit()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileuploadquestion")
                .WithMaxFileSize(2)
                .WithMaxCombinedFileSize(4)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {$"fileuploadquestion{FileUploadConstants.SUFFIX}", new List<DocumentModel> { new DocumentModel { FileSize = 1048576 } } }
            };
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenMultiple_TotalFiles_AreBelow_MaxLimit()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileuploadquestion")
                .WithMaxFileSize(2)
                .WithMaxCombinedFileSize(4)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    $"fileuploadquestion{FileUploadConstants.SUFFIX}", new List<DocumentModel> {
                    new DocumentModel { FileSize = 1048576 },
                    new DocumentModel { FileSize = 1048576 },
                    new DocumentModel { FileSize = 1048576 } }
                }
            };
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenTotalFiles_AreBelow_MaxLimit_When_GettingAdditonalFiles_FromSavedAnswers()
        {
            _mockSessionHelper.Setup(_ => _.GetBrowserSessionId())
                .Returns("12345");

            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.Is<string>(_ => _ == "12345")))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers> { new Answers { QuestionId = "fileuploadquestion", Response = Newtonsoft.Json.JsonConvert.SerializeObject(new List<FileUploadModel> { new FileUploadModel { FileSize = 1048576 } }) } } } } }));

            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileuploadquestion")
                .WithMaxFileSize(2)
                .WithMaxCombinedFileSize(4)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    $"fileuploadquestion{FileUploadConstants.SUFFIX}", new List<DocumentModel> {
                    new DocumentModel { FileSize = 1048576 },
                    new DocumentModel { FileSize = 1048576 } }
                },
                { "Path", "page-one"}
            };
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_When_Single_TotalFiles_AreOver_MaxLimit()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileuploadquestion")
                .WithMaxFileSize(2)
                .WithMaxCombinedFileSize(4)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    $"fileuploadquestion{FileUploadConstants.SUFFIX}", new List<DocumentModel> {
                    new DocumentModel { FileSize = 6048576 }
                }
                }
            };

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            Assert.False(result.IsValid);
            Assert.Equal($"The total size of all your added files must not be more than 4MB", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_When_Multiple_TotalFiles_AreOver_MaxLimit()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileuploadquestion")
                .WithMaxFileSize(2)
                .WithMaxCombinedFileSize(4)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    $"fileuploadquestion{FileUploadConstants.SUFFIX}", new List<DocumentModel> {
                    new DocumentModel { FileSize = 3048576 },
                    new DocumentModel { FileSize = 3048576 }
                }
                }
            };

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            Assert.False(result.IsValid);
            Assert.Equal($"The total size of all your added files must not be more than 4MB", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_WhenTotalFiles_AreOver_MaxLimit_When_GettingAdditonalFiles_FromSavedAnswers()
        {
            _mockSessionHelper.Setup(_ => _.GetBrowserSessionId())
                .Returns("12345");

            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.Is<string>(_ => _ == "12345")))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers> { new Answers { QuestionId = $"fileuploadquestion{FileUploadConstants.SUFFIX}", Response = Newtonsoft.Json.JsonConvert.SerializeObject(new List<FileUploadModel> { new FileUploadModel { FileSize = 2048576 } }) } } } } }));

            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileuploadquestion")
                .WithMaxFileSize(2)
                .WithMaxCombinedFileSize(5)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    $"fileuploadquestion{FileUploadConstants.SUFFIX}", new List<DocumentModel> {
                    new DocumentModel { FileSize = 2048576 },
                    new DocumentModel { FileSize = 2048576 } }
                },
                { "Path", "page-one"}
            };
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
            Assert.False(result.IsValid);
            Assert.Equal($"The total size of all your added files must not be more than 5MB", result.Message);
        }
    }
}
