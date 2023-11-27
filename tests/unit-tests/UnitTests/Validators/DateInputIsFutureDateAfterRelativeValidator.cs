using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.RelativeDateHelper;
using form_builder.Validators;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class DateInputIsFutureDateAfterRelativeValidatorTests
    {
        private readonly DateInputIsFutureDateAfterRelativeValidator _dateInputIsFutureDateAfterRelativeValidator;

        public DateInputIsFutureDateAfterRelativeValidatorTests()
        {
            _dateInputIsFutureDateAfterRelativeValidator = new (new RelativeDateHelper());
        }

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

            element.Properties.IsFutureDateAfterRelative = "3-d-ex";
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
            var exclusiveElement = new ElementBuilder()
               .WithType(EElementType.DateInput)
               .WithQuestionId("test-date")
               .WithIsFutureDateAfterRelative("3-d-ex", "Date is too late")
               .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-d-in", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new form_builder.Models.FormSchema());
            var inclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(exclusiveResult.IsValid);
            Assert.Equal("Date is too late", exclusiveResult.Message);

            Assert.False(inclusiveResult.IsValid);
            Assert.Equal("Date is too late", inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenDayIsFarAwayEnough()
        {
            // Arrange
            var exclusiveElement = new ElementBuilder()
               .WithType(EElementType.DateInput)
               .WithQuestionId("test-date")
               .WithIsFutureDateAfterRelative("3-d-ex", "Date is too soon")
               .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-d-in", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(4);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new form_builder.Models.FormSchema());
            var inclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(exclusiveResult.IsValid);
            Assert.Equal(string.Empty, exclusiveResult.Message);

            Assert.True(inclusiveResult.IsValid);
            Assert.Equal(string.Empty, inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenDayIsEqualRelativeDate_Exclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-d-ex", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too soon", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenDayIsEqualRelativeDate_Inclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-d-in", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
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
            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-m-ex", "Date is too soon")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-m-in", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1).AddYears(1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new form_builder.Models.FormSchema());
            var inclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(exclusiveResult.IsValid);
            Assert.Equal(string.Empty, exclusiveResult.Message);

            Assert.True(inclusiveResult.IsValid);
            Assert.Equal(string.Empty, inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenMonthIsTooCloseDateEntered()
        {
            // Arrange
            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-m-ex", "Date is too soon")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-m-in", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1).AddMonths(2);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new form_builder.Models.FormSchema());
            var inclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(exclusiveResult.IsValid);
            Assert.Equal("Date is too soon", exclusiveResult.Message);

            Assert.False(inclusiveResult.IsValid);
            Assert.Equal("Date is too soon", inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsTooCloseDateEntered()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("2-y-ex", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1).AddYears(1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too soon", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenMonthIsEqualRelativeDate_Exclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-m-ex", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddMonths(3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too soon", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenMonthIsEqualRelativeDate_Inclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-m-in", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddMonths(3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenYearIsFarEnoughAway()
        {
            // Arrange
            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-y-ex", "Date is too soon")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-y-in", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1).AddYears(10);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new form_builder.Models.FormSchema());
            var inclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(exclusiveResult.IsValid);
            Assert.Equal(string.Empty, exclusiveResult.Message);

            Assert.True(inclusiveResult.IsValid);
            Assert.Equal(string.Empty, inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsTooClose()
        {
            // Arrange
            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-y-ex", "Date is too soon")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-y-in", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddYears(1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new form_builder.Models.FormSchema());
            var inclusiveResult = _dateInputIsFutureDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(exclusiveResult.IsValid);
            Assert.Equal("Date is too soon", exclusiveResult.Message);

            Assert.False(inclusiveResult.IsValid);
            Assert.Equal("Date is too soon", inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsEqualRelativeDate_Exclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-y-ex", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddYears(3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too soon", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenYearIsEqualRelativeDate_Inclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsFutureDateAfterRelative("3-y-in", "Date is too soon")
                .Build();

            var dateToCheck = DateTime.Now.AddYears(3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
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
                .WithIsFutureDateAfterRelative("2-d-ex", "")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsFutureDateAfterRelativeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }
    }
}
