using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class DateInputIsFutureDateAfterRelativeValidatorTests
    {
        private readonly DateInputIsFutureDateAfterRelativeValidator _dateInputIsFutureDateAfterRelativeValidator = new DateInputIsFutureDateAfterRelativeValidator();

        [Fact]
        public void Validate_ShouldCheck_TheElementTypePassedIsNotADateInputElement()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, null, new form_builder.Models.FormSchema());

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

            element.Properties.IsFutureDateAfterRelative = "3-d";
            element.Properties.Optional = true;

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenDayIsTooCloseDateEntered()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-d", "Date is too soon")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", DateTime.Now.AddDays(1).Day.ToString()},
                {"test-date-month", DateTime.Now.Month.ToString()},
                {"test-date-year", DateTime.Now.Year.ToString()}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too soon", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenDayIsFarAwayEnough()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-d", "Date is too soon")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", DateTime.Now.AddDays(4).Day.ToString()},
                {"test-date-month", DateTime.Now.Month.ToString()},
                {"test-date-year", DateTime.Now.Year.ToString()}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenMonthIsFarAwayEnough()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-m", "Date is too soon")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", DateTime.Now.AddDays(1).Day.ToString()},
                {"test-date-month", DateTime.Now.Month.ToString()},
                {"test-date-year", DateTime.Now.AddYears(1).Year.ToString()}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenMonthIsTooCloseDateEntered()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-m", "Date is too soon")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", DateTime.Now.AddDays(1).Day.ToString()},
                {"test-date-month", DateTime.Now.AddMonths(2).Month.ToString()},
                {"test-date-year", DateTime.Now.Year.ToString()}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too soon", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsTooCloseDateEntered()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("2-y", "Date is too soon")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", DateTime.Now.AddDays(1).Day.ToString()},
                {"test-date-month", DateTime.Now.Month.ToString()},
                {"test-date-year", DateTime.Now.AddYears(1).Year.ToString()}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too soon", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenYearIsFarEnoughAway()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("2-y", "Date is too soon")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", DateTime.Now.AddDays(1).Day.ToString()},
                {"test-date-month", DateTime.Now.Month.ToString()},
                {"test-date-year", DateTime.Now.AddYears(10).Year.ToString()}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowDefaultValidationMessage_WhenValidationIsTriggeredAndNoValidationMessageIsSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("2-d", "")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", DateTime.Now.AddDays(1).Day.ToString()},
                {"test-date-month", DateTime.Now.Month.ToString()},
                {"test-date-year", DateTime.Now.Year.ToString()}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }
    }
}
