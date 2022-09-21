using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks.Elements;
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
            var element = new ElementBuilder()
                .WithPropertyText("label text")
                .WithType(EElementType.UploadedFilesSummary)
                .WithFileUploadQuestionIds(new List<string>())
                .Build();

            // Act
            var check = new UploadedFilesSummaryQuestionsIsSetCheck();
            var result = check.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));

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
            var result = check.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void CheckUploadedFilesSummaryQuestionsIsSet_ShouldNot_ThrowException_WhenElement_Has_RequiredFileUpload_QuestionIds()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("question")
                .WithPropertyText("label text")
                .WithType(EElementType.UploadedFilesSummary)
                .WithFileUploadQuestionIds(new List<string> { "question-one" })
                .Build();

            // Act
            var check = new UploadedFilesSummaryQuestionsIsSetCheck();
            var result = check.Validate(element);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}