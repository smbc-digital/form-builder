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
    public class UploadedFileSummaryQuestionIsSetCheckTests
    {
        [Fact]
        public void UploadedFilesSummaryQuestionsIsSet_IsNotValid_WhenElementDoNotContain_RequiredFileUpload_QuestionIds()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithPropertyText("label text")
                .WithType(EElementType.UploadedFilesSummary)
                .WithFileUploadQuestionIds(new List<string>())
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            // Act
            var check = new UploadedFilesSummaryQuestionsIsSetCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Any(_ => _.Equals($"FAILURE - Uploaded Files Summary Question Is Set, Uploaded files summary must have atleast one file questionId specified to display the list of uploaded files.")));
        }

        [Fact]
        public void CheckUploadedFilesSummaryQuestionsIsSet_ShouldThrowException_WhenElementDoNotContain_Required_Text()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.UploadedFilesSummary)
                .WithPropertyText(string.Empty)
                .WithFileUploadQuestionIds(new List<string> { "" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            // Act
            var check = new UploadedFilesSummaryQuestionsIsSetCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Any(_ => _.Equals($"FAILURE - Uploaded Files Summary Question Is Set, Uploaded files summary text must not be empty.")));
        }

        [Fact]
        public void CheckUploadedFilesSummaryQuestionsIsSet_ShouldNot_ThrowException_WhenElement_Has_RequiredFileUpload_QuestionIds()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithQuestionId("question")
                .WithPropertyText("label text")
                .WithType(EElementType.UploadedFilesSummary)
                .WithFileUploadQuestionIds(new List<string> { "question-one" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            // Act
            var check = new UploadedFilesSummaryQuestionsIsSetCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}