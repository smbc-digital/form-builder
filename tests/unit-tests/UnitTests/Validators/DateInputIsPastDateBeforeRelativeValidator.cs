using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using form_builder.Models;
using Xunit;
using Moq;
using form_builder.Helpers.RelativeDateHelper;

namespace form_builder_tests.UnitTests.Validators
{
    public class DateInputIsPastDateBeforeRelativeValidatorTests
    {
        private readonly DateInputIsPastDateBeforeRelativeValidator _dateInputIsPastDateBeforeRelativeValidator;

        public DateInputIsPastDateBeforeRelativeValidatorTests()
        {
            _dateInputIsPastDateBeforeRelativeValidator = new (new RelativeDateHelper());
        }

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
            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-d-ex", "Date is too late")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-d-in", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(-4);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());
            var inclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());

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
                .WithIsPastDateBeforeRelative("3-d-ex", "Date is too late")
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
        public void Validate_ShouldNotShowValidationMessage_WhenDayIsEqualRelativeDate_Inclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-d-in", "Date is too late")
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
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenDayIsAfterRelativeDate()
        {
            // Arrange
            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-d-ex", "Date is too late")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-d-in", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(-2);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());
            var inclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());

            // Assert
            Assert.False(exclusiveResult.IsValid);
            Assert.Equal("Date is too late", exclusiveResult.Message);

            Assert.False(inclusiveResult.IsValid);
            Assert.Equal("Date is too late", inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenMonthIsBeforeRelativeDate()
        {
            // Arrange
            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-m-ex", "Date is too late")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-m-in", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddDays(-1).AddMonths(-3);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());
            var inclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());

            // Assert
            Assert.True(exclusiveResult.IsValid);
            Assert.Equal(string.Empty, exclusiveResult.Message);

            Assert.True(inclusiveResult.IsValid);
            Assert.Equal(string.Empty, inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenMonthIsEqualRelativeDate_Exclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-m-ex", "Date is too late")
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
        public void Validate_ShouldNotShowValidationMessage_WhenMonthIsEqualRelativeDate_Inclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-m-in", "Date is too late")
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
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenMonthIsAfterRelativeDate()
        {
            // Arrange
            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-m-ex", "Date is too late")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("3-m-in", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddMonths(-2);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());
            var inclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());

            // Assert
            Assert.False(exclusiveResult.IsValid);
            Assert.Equal("Date is too late", exclusiveResult.Message);

            Assert.False(inclusiveResult.IsValid);
            Assert.Equal("Date is too late", inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenYearBeforeRelativeDate()
        {
            // Arrange
            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("2-y-ex", "Date is too late")
                .Build();

            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("2-y-in", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddYears(-2).AddDays(-1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var inclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());
            var exclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());

            // Assert
            Assert.True(inclusiveResult.IsValid);
            Assert.Equal(string.Empty, inclusiveResult.Message);

            Assert.True(exclusiveResult.IsValid);
            Assert.Equal(string.Empty, exclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsAfterRelativeDate()
        {
            // Arrange
            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("2-y-ex", "Date is too late")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("2-y-in", "Date is too late")
                .Build();

            var dateToCheck = DateTime.Now.AddYears(-1);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var inclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());
            var exclusiveResult = _dateInputIsPastDateBeforeRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());

            // Assert
            Assert.False(exclusiveResult.IsValid);
            Assert.Equal("Date is too late", exclusiveResult.Message);

            Assert.False(inclusiveResult.IsValid);
            Assert.Equal("Date is too late", inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsEqualRelativeDate_Exclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("2-y-ex", "Date is too late")
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
        public void Validate_ShouldNotShowValidationMessage_WhenYearIsEqualRelativeDate_Inclusive()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateBeforeRelative("2-y-in", "Date is too late")
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
                .WithIsPastDateBeforeRelative("2-d-ex", "")
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
