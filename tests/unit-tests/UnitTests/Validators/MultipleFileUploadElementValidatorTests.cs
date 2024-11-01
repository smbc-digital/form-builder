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
    public class MultipleFileUploadElementValidatorTests
    {
        private readonly MultipleFileUploadElementValidator _fileUploadElementValidatorTest;
        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCacheWrapper = new Mock<IDistributedCacheWrapper>();

        public MultipleFileUploadElementValidatorTests()
        {
            _fileUploadElementValidatorTest = new MultipleFileUploadElementValidator(_mockSessionHelper.Object, _mockDistributedCacheWrapper.Object);
        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenNot_MultipleFileUpload_ElementType()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _fileUploadElementValidatorTest.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenQuestionNo_WithinViewModel()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileUpload")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"{element.Properties.QuestionId}-fileupload", new List<DocumentModel>() }
            };

            // Act
            var result = _fileUploadElementValidatorTest.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
            _mockSessionHelper.Verify(_ => _.GetBrowserSessionId(), Times.Never);
            _mockDistributedCacheWrapper.Verify(_ => _.GetString(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Validate_ShouldReturnFalseForValidationResult_WhenValueInPageAnswerIsNull()
        {
            // Arrange       
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileUpload")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _fileUploadElementValidatorTest.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenValueIn_PageAnswer_IsNotNull()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileUpload")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    $"fileUpload{FileUploadConstants.SUFFIX}", new List<DocumentModel>()
                },
                { "Path", "page-one"}
            };

            _mockSessionHelper.Setup(_ => _.GetBrowserSessionId()).Returns("12345");
            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.Is<string>(_ => _ == "12345")))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers> { new Answers { QuestionId = "fileUpload", Response = Newtonsoft.Json.JsonConvert.SerializeObject(new List<FileUploadModel> { new FileUploadModel { FileSize = 2048576 } }) } } } } }));

            // Act
            var result = _fileUploadElementValidatorTest.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldNot_RunValidator_WhenOptional_And_SubmitButtonClicked()
        {
            // Arrange       
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileUpload")
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { ButtonConstants.SUBMIT, ButtonConstants.SUBMIT }
            };

            // Act
            var result = _fileUploadElementValidatorTest.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
