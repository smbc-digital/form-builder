using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class AcceptedFileUploadFileTypesCheckTests
    {
        [Fact]
        public void AcceptedFileUploadFileTypesCheck_IsValid_WhenNoFileUploadElementsExists()
        {
            // Arrange
            var pages = new List<Page>();
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("textBox")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("textArea")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .Build();
            
            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            // Act
            var check = new AcceptedFileUploadFileTypesCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void AcceptedFileUploadFileTypesCheck_IsNotValid_WhenAcceptedFleTypes_HasInvalidExtensionName()
        {
            // Arrange
            var pages = new List<Page>();
            var invalidElementQuestionId = "fileUpload2";

            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId("fileUpload")
                .WithAcceptedMimeType(".png")
                .Build();

            var elementWithInvalidMimeType = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId(invalidElementQuestionId)
                .WithAcceptedMimeType("png")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(elementWithInvalidMimeType)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            // Act
            var check = new AcceptedFileUploadFileTypesCheck();
            var result = check.Validate(schema);

            // Act & Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Messages, messages => messages.Equals($"FAILURE - Accepted FileUpload File Types Check, Allowed file type in FileUpload element in form 'test-name', 'fileUpload2' must have a valid extension which begins with a '.', e.g. .png"));
        }

        [Fact]
        public void AcceptedFileUploadFileTypesCheck_IsValid_WhenAllFileTypesAreValid()
        {
            // Arrange
            var pages = new List<Page>();
            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithAcceptedMimeType(".png")
                .WithAcceptedMimeType(".pdf")
                .WithAcceptedMimeType(".jpg")
                .WithQuestionId("fileUpload")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithAcceptedMimeType(".png")
                .WithAcceptedMimeType(".jpg")
                .WithAcceptedMimeType(".jpge")
                .WithQuestionId("fileUpload1")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithAcceptedMimeType(".docx")
                .WithQuestionId("fileUpload3")
                .Build();

            var element4 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithAcceptedMimeType(".docx")
                .WithAcceptedMimeType(".doc")
                .WithQuestionId("fileUpload4")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element3)
                .WithElement(element4)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .WithPage(page2)
                .Build();

            // Act
            var check = new AcceptedFileUploadFileTypesCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}