using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictPastDateValidatorTests
    {
        private readonly RestrictPastDateValidator _restrictPastDateValidator = new RestrictPastDateValidator();

        [Fact]
        public void Validate_ShouldCheckRestrictPastDatePropertyIsNotSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .Build();

            // Act
            var result = _restrictPastDateValidator.Validate(element, null, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ReturnsTrueWhenOptionalFieldsAreEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithRestrictPastDate(true)
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _restrictPastDateValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenFieldsAreEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithRestrictPastDate(true)
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _restrictPastDateValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenPastDateEntered()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithRestrictPastDate(true, "Past date validation message")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", "01"},
                {"test-date-month", "01"},
                {"test-date-year", "2010"}
            };

            // Act
            var result = _restrictPastDateValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Past date validation message", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnTrueWhenPastDateIsNotEntered()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithRestrictPastDate(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", "10"},
                {"test-date-month", "10"},
                {"test-date-year", "2030"}
            };

            // Act
            var result = _restrictPastDateValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }
    }
}