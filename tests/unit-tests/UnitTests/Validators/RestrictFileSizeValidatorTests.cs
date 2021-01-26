using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictFileSizeValidatorTests
    {
        private readonly RestrictFileSizeValidator _validator = new RestrictFileSizeValidator();

        [Theory]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void Validate_ShouldReturn_True_ValidationResult_WhenNot_FileUploadElement(EElementType type)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .Build();

            // Act
            var result = _validator.Validate(element, null, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }


        [Theory]
        [InlineData(EElementType.FileUpload)]
        [InlineData(EElementType.MultipleFileUpload)]
        public void Validate_ShouldReturn_True_ValidationResult_WhenQuestion_NotIn_ViewModel(EElementType type)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId("fileuploadquestion")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }


        [Theory]
        [InlineData(EElementType.FileUpload)]
        [InlineData(EElementType.MultipleFileUpload)]
        public void Validate_ShouldReturn_True_ValidationResult_When_ValueIsNull_InViewModel_ForQuestion(EElementType type)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId("fileuploadquestion")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"fileuploadquestion{FileUploadConstants.SUFFIX}", null}
            };

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(EElementType.FileUpload, 0)]
        [InlineData(EElementType.MultipleFileUpload, 0)]
        [InlineData(EElementType.FileUpload, 2)]
        [InlineData(EElementType.MultipleFileUpload, 2)]
        public void Validate_ShouldReturn_True_ValidationResult_When_Flle_IsUnder_MaxSizeLimit(EElementType type, int maxSize)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId("fileuploadquestion")
                .WithMaxFileSize(maxSize)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"fileuploadquestion{FileUploadConstants.SUFFIX}", new List<DocumentModel> { new DocumentModel { FileSize = 1048576 } }}
            };

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(EElementType.FileUpload, 0, 10)]
        [InlineData(EElementType.MultipleFileUpload, 0, 10)]
        [InlineData(EElementType.FileUpload, 2, 2)]
        [InlineData(EElementType.MultipleFileUpload, 2, 2)]
        public void Validate_ShouldReturn_False_ValidationResult_When_File_IsOver_MaxSizeLimit(EElementType type, int maxSize, int maxFileSize)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId("fileuploadquestion")
                .WithMaxFileSize(maxSize)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"fileuploadquestion{FileUploadConstants.SUFFIX}", new List<DocumentModel> { new DocumentModel { FileSize = 25117248 } }}
            };

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal($"The selected file must be smaller than {maxFileSize}MB", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_When_Single_File_InMultipleUpload_IsOverLimit()
        {
            // Arrange
            var fileName = "test.jpg";
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileuploadquestion")
                .WithMaxFileSize(3)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"fileuploadquestion{FileUploadConstants.SUFFIX}", new List<DocumentModel> {
                    new DocumentModel { FileSize = 1048576 } ,
                    new DocumentModel { FileSize = 1048576 },
                    new DocumentModel { FileSize = 4048576, FileName = fileName }
                }}
            };

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal($"{fileName} must be smaller than 3MB", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_When_Multiple_Files_InMultipleUpload_IsOverLimit()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId("fileuploadquestion")
                .WithMaxFileSize(3)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"fileuploadquestion{FileUploadConstants.SUFFIX}", new List<DocumentModel> {
                    new DocumentModel { FileSize = 4048576, FileName = "file1.txt" },
                    new DocumentModel { FileSize = 1048576, FileName = "file2.txt" },
                    new DocumentModel { FileSize = 4048576, FileName = "file3.txt" }
                }}
            };

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal($"file3.txt must be smaller than 3MB <br/> file1.txt must be smaller than 3MB", result.Message);
        }
    }
}