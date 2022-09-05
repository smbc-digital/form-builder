using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class DateInputRangeCheckTests
    {

        [Fact]
        public void DateInputRangeWithOutsideRange_IsValid_WhenFormatIsCorrect()
        {
            // Arrange
            var dateinput = new ElementBuilder()
           .WithType(EElementType.DateInput)
           .WithQuestionId("testDate")
           .WithOutsideRange("18-Y")
           .Build();

            var page1 = new PageBuilder()
               .WithElement(dateinput)
               .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .Build();
            // Act
            DateInputRangeCheck check = new();
            var result = check.Validate(schema);
            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void DateInputRangeWithOutsideRange_IsNotValid_WhenFormatIsNotCorrect()
        {
            // Arrange
            var dateinput = new ElementBuilder()
           .WithType(EElementType.DateInput)
           .WithQuestionId("testDate")
           .WithOutsideRange("18-W")
           .Build();

            var page1 = new PageBuilder()
               .WithElement(dateinput)
               .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .Build();
            // Act
            DateInputRangeCheck check = new();
            var result = check.Validate(schema);
            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages.Where(message => message.StartsWith("FAILURE - The provided json has date input element with a incorrect outside range value for 'testDate'")));
        }

        [Fact]
        public void DateInputWithinRange_IsValid_WhenFormatIsCorrect()
        {
            // Arrange
            var dateinput = new ElementBuilder()
           .WithType(EElementType.DateInput)
           .WithQuestionId("testDate")
           .WithWithinRange("18-D")
           .Build();

            var page1 = new PageBuilder()
               .WithElement(dateinput)
               .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .Build();
            // Act
            DateInputRangeCheck check = new();
            var result = check.Validate(schema);
            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void DateInputWithinRange_IsNotValid_WhenFormatIsNotCorrect()
        {
            // Arrange
            var dateinput = new ElementBuilder()
           .WithType(EElementType.DateInput)
           .WithQuestionId("testDate")
           .WithWithinRange("18-W")
           .Build();

            var page1 = new PageBuilder()
               .WithElement(dateinput)
               .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .Build();
            // Act
            DateInputRangeCheck check = new();
            var result = check.Validate(schema);
            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages.Where(message => message.StartsWith("FAILURE - The provided json has date input element with a incorrect within range value for 'testDate'")));
        }
    }
}
