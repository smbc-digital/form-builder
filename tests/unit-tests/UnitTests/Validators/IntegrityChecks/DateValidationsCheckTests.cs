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
        public void DateValidationsCheck_BeforeAndAfter_IsNotValid(
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
        public void DateValidationsCheck_BeforeAndAfter_IsValid(
            string questionId,
            string isDateBefore,
            string isDatefter)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
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

        [Theory]
        [InlineData("3-d-ex")]
        [InlineData("3-D-EX")]
        [InlineData("3-d-in")]
        [InlineData("3-D-IN")]
        [InlineData("3-m-in")]
        [InlineData("3-M-IN")]
        [InlineData("3-m-ex")]
        [InlineData("3-M-EX")]
        [InlineData("3-y-in")]
        [InlineData("3-Y-IN")]
        [InlineData("3-y-ex")]
        [InlineData("3-Y-EX")]
        [InlineData("999-y-ex")]
        [InlineData("1-Y-ex")]
        [InlineData("1-y-iN")]
        public void DateValidationsCheck_RelativeDate_IsValid(
            string relativeDateString)
        {
            // Arrange
            var futureDateAfterRelativeElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("dateInput")
                .WithIsFutureDateAfterRelative(relativeDateString, "message")
                .Build();

            var futureDateBeforeRelativeElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("dateInput")
                .WithIsFutureDateBeforeRelative(relativeDateString, "message")
                .Build();

            var pastDateBeforeRelativeElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("dateInput")
                .WithIsPastDateBeforeRelative(relativeDateString, "message")
                .Build();

            // Act
            DateValidationsCheck check = new();
            var futureDateAfterRelativeResult = check.Validate(futureDateAfterRelativeElement);
            var futureDateBeforeRelativeResult = check.Validate(futureDateBeforeRelativeElement);
            var pastDateBeforeRelativeRelativeResult = check.Validate(pastDateBeforeRelativeElement);

            // Assert
            Assert.True(futureDateAfterRelativeResult.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, futureDateAfterRelativeResult.Messages);

            Assert.True(futureDateBeforeRelativeResult.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, futureDateBeforeRelativeResult.Messages);

            Assert.True(pastDateBeforeRelativeRelativeResult.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, pastDateBeforeRelativeRelativeResult.Messages);
        }

        [Theory]
        [InlineData("badString")]
        [InlineData("3-f-in")]
        [InlineData("3-m")]
        [InlineData("y-in")]
        [InlineData("-1-d-ex")]
        public void DateValidationsCheck_FutureDateAfterRelative_IsNotValid(
            string relativeDateString)
        {
            // Arrange
            var futureDateAfterRelativeElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("dateInput")
                .WithIsFutureDateAfterRelative(relativeDateString, "message")
                .Build();

            var futureDateBeforeRelativeElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("dateInput")
                .WithIsFutureDateBeforeRelative(relativeDateString, "message")
                .Build();

            var pastDateBeforeRelativeElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("dateInput")
                .WithIsPastDateBeforeRelative(relativeDateString, "message")
                .Build();

            // Act
            DateValidationsCheck check = new();
            var futureDateAfterRelativeResult = check.Validate(futureDateAfterRelativeElement);
            var futureDateBeforeRelativeResult = check.Validate(futureDateBeforeRelativeElement);
            var pastDateBeforeRelativeRelativeResult = check.Validate(pastDateBeforeRelativeElement);

            // Assert
            Assert.False(futureDateAfterRelativeResult.IsValid);
            Assert.Collection<string>(futureDateAfterRelativeResult.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));

            Assert.False(futureDateBeforeRelativeResult.IsValid);
            Assert.Collection<string>(futureDateBeforeRelativeResult.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));

            Assert.False(pastDateBeforeRelativeRelativeResult.IsValid);
            Assert.Collection<string>(pastDateBeforeRelativeRelativeResult.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}