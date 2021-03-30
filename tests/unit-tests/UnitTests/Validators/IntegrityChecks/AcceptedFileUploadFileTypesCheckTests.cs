using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class AcceptedFileUploadFileTypesCheckTests
    {
        [Fact]
        public void AcceptedFileUploadFileTypesCheck_IsValid_WhenNoFileUploadElementsExists()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("textBox")
                .Build();

            // Act
            AcceptedFileUploadFileTypesCheck check = new();
            var result = check.Validate(element);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }

        [Theory]
        [InlineData("png", ".pdf")]
        [InlineData(".doc", "docx")]
        public void AcceptedFileUploadFileTypesCheck_IsNotValid_WhenAcceptedFleTypes_HasInvalidExtensionName(string mimeType1, string mimeType2)
        {
            // Arrange
            var invalidElementQuestionId = "fileUpload2";

            var elementWithInvalidMimeType = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId(invalidElementQuestionId)
                .WithAcceptedMimeType(mimeType1)
                .WithAcceptedMimeType(mimeType2)
                .Build();


            // Act
            AcceptedFileUploadFileTypesCheck check = new();
            var result = check.Validate(elementWithInvalidMimeType);

            // Act & Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Theory]
        [InlineData(".png", ".pdf")]
        [InlineData(".pdf", ".jpg")]
        [InlineData(".jpg", ".jpge")]
        [InlineData(".docx",".doc")]
        [InlineData(".doc", ".png")]
        public void AcceptedFileUploadFileTypesCheck_IsValid_WhenFileTypeAreValid(string mimeType1, string mimeType2)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithAcceptedMimeType(mimeType1)
                .WithAcceptedMimeType(mimeType2)
                .WithQuestionId("fileUpload")
                .Build();

            // Act
            AcceptedFileUploadFileTypesCheck check = new();
            var result = check.Validate(element);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}