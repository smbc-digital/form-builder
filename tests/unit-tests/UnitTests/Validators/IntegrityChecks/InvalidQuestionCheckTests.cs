using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class InvalidQuestionCheckTests
    {
        [Theory]
        [InlineData("validquestionId")]
        public void QuestionIsValid_WhenQuestionIdValid(string questionId)
        {
            // Arrange
            var validElement = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .Build();

            var check = new InvalidQuestionCheck();

            // Act
            var result = check.Validate(validElement);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }

        [Theory]
        [InlineData("invalid-questionId")]
        [InlineData("question4")]
        [InlineData("question£")]
        [InlineData("que!stion")]
        [InlineData("quest%ion")]
        [InlineData("question.")]
        [InlineData(".question")]
        public void InvalidQuestion_IsNotValid_WhenQuestionIdInvalid(string questionId)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .Build();

            var check = new InvalidQuestionCheck();

            // Act
            var result = check.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Theory]
        [InlineData("valid.question")]
        [InlineData("valid.question.id")]
        public void CheckForInvalidQuestion_ShouldNotThrowExceptionForValidQuestionId(string questionId)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .Build();

            var check = new InvalidQuestionCheck();

            // Act
            var result = check.Validate(element);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}