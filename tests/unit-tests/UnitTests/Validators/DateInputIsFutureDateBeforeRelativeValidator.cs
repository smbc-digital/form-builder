using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class DateInputIsFutureDateBeforeRelativeValidatorTests
    {
        private readonly DateInputIsFutureDateBeforeRelativeValidator _dateInputIsFutureDateBeforeRelativeValidator = new DateInputIsFutureDateBeforeRelativeValidator();

        [Fact]
        public void Validate_ShouldCheck_TheElementTypePassedIsNotADateInputElement()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            // Act
            var result = _dateInputIsFutureDateBeforeRelativeValidator.Validate(element, null, new form_builder.Models.FormSchema());

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

            element.Properties.IsFutureDateBeforeRelative = "3-d";
            element.Properties.Optional = true;

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _dateInputIsFutureDateBeforeRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenDayIsCloseEnough()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateBeforeRelative("3-d", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateBeforeRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenDayIsTooFarAway()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateBeforeRelative("3-d", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(4);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateBeforeRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenMonthIsTooFarAway()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateBeforeRelative("3-m", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1).AddYears(1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateBeforeRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenMonthIsCloseEnough()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateBeforeRelative("3-m", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1).AddMonths(2);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateBeforeRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenYearIsCloseEnough()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateBeforeRelative("2-y", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1).AddYears(1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateBeforeRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsTooFarAway()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateBeforeRelative("2-y", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1).AddYears(10);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateBeforeRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

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
                .WithIsFutureDateBeforeRelative("2-d", "")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateBeforeRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }
    }
}
