using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class DateValidationsCheckTests
    {
        [Theory]
        [InlineData("test", "test", "")]
        [InlineData("test", "", "test")]
        public void DateValidationsCheck_IsNotValid(
            string questionId,
            string isDateBefore,
            string isDatefter)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId(questionId)
                .WithIsDateBefore(isDateBefore)
                .WithIsDateAfter(isDatefter)
                .Build();

            // Act
            DateValidationsCheck check = new();
            var result = check.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Theory]
        [InlineData("test", "not-same", "")]
        [InlineData("test", "", "not-same")]
        public void DateValidationsCheck_IsValid(
            string questionId,
            string isDateBefore,
            string isDatefter)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId(questionId)
                .WithIsDateBefore(isDateBefore)
                .WithIsDateAfter(isDatefter)
                .Build();

            // Act
            DateValidationsCheck check = new();
            var result = check.Validate(element);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}