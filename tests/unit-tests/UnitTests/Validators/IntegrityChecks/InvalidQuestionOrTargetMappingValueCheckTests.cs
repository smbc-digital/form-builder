using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class InvalidQuestionOrTargetMappingValueCheckTests
    {
        [Theory]
        [InlineData("invalid-questionId", "")]
        [InlineData("question4", "")]
        [InlineData("question£", "")]
        [InlineData("que!stion", "")]
        [InlineData("quest%ion", "")]
        [InlineData("question.", "")]
        [InlineData(".question", "")]
        [InlineData("", "invalid-tagretMapping")]
        [InlineData("", "tagret4")]
        [InlineData("", "target$")]
        [InlineData("", "target.")]
        [InlineData("", ".target")]
        public void
        InvalidQuestionOrTargetMappingValueCheck_IsNotValid_When_InvalidQuestionId_OrTargetMapping(
                string questionId, string targetMapping)
        {
            // Arrange
            var validElement = new ElementBuilder()
                .WithQuestionId("question")
                .WithType(EElementType.Textarea)
                .Build();

            var element2 = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .WithTargetMapping(targetMapping)
                .Build();

            var page = new PageBuilder()
                .WithElement(validElement)
                .WithElement(element2)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(validElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(page2)
                .WithName("test-name")
                .Build();

            var check = new InvalidQuestionOrTargetMappingValueCheck();

            // Act
            var result = check.Validate(schema);
            
            // Assert
            Assert.False(result.IsValid);
            Assert.Collection(result.Messages, message => Assert.Contains("FAILURE - The provided json 'test-name' contains invalid QuestionIDs or TargetMapping", message));
        }

        [Theory]
        [InlineData("validquestionId", "")]
        [InlineData("valid.question", "")]
        [InlineData("valid.question.id", "")]
        [InlineData("", "validtagretMapping")]
        [InlineData("", "valid.target")]
        [InlineData("", "valid.target.mapping")]
        public void
        CheckForInvalidQuestionOrTargetMappingValue_ShouldNotThrowExceptionWhen_ValidQuestionId_OrTargetMapping(
                string questionId, string targetMapping)
        {
            // Arrange
            var validElement = new ElementBuilder()
                .WithQuestionId("question")
                .WithType(EElementType.Textarea)
                .Build();

            var element2 = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .WithTargetMapping(targetMapping)
                .Build();

            var page = new PageBuilder()
                .WithElement(validElement)
                .WithElement(element2)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(validElement)
                .Build();

            // Act & Assert
            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(page2)
                .WithName("test-name")
                .Build();

            var check = new InvalidQuestionOrTargetMappingValueCheck();

            // Act
            var result = check.Validate(schema);
            
            // Assert
            Assert.True(result.IsValid);
        }
    }
}