using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using form_builder.Models;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class DateInputIsPastDateBeforeRelativeValidatorTests
    {
        private readonly DateInputIsPastDateBeforeRelativeValidator _dateInputIsPastDateBeforeRelativeValidator = new();

        [Fact]
        public void Validate_ShouldCheck_TheElementTypePassedIsNotADateInputElement()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, null, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldCheck_DateIsOptional()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .Build();

            element.Properties.IsPastDateBeforeRelative = "3-d";
            element.Properties.Optional = true;

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenDayIsBeforeRelativeDate()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-d", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(-4);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenDayIsEqualRelativeDate()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-d", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(-3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenDayIsAfterRelativeDate()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-d", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(-2);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenMonthIsBeforeRelativeDate()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-m", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(-1).AddMonths(-3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(String.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenMonthIsEqualRelativeDate()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-m", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddMonths(-3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenMonthIsAfterRelativeDate()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-m", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddMonths(-2);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenYearBeforeRelativeDate()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("2-y", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddYears(-2).AddDays(-1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsAfterRelativeDate()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("2-y", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddYears(-1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsEqualRelativeDate()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("2-y", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddYears(-2);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }
  

        [Fact]
        public void Validate_ShouldShowDefaultValidationMessage_WhenValidationIsTriggeredAndNoValidationMessageIsSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("2-d", "")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(-1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateBeforeRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }
    }
}
