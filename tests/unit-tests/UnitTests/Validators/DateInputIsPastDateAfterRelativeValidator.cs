using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using form_builder.Models;
using Xunit;
using Moq;
using form_builder.Helpers.RelativeDateHelper;
using form_builder.Constants;
using form_builder.Models.Elements;
using StockportGovUK.NetStandard.Gateways.Models.RevsAndBens;

namespace form_builder_tests.UnitTests.Validators
{
    public class DateInputIsPastDateAfterRelativeValidatorTests
    {
        private readonly Mock<IRelativeDateHelper> _mockRelativeDateHelper = new Mock<IRelativeDateHelper>();
        private readonly DateInputIsPastDateAfterRelativeValidator _dateInputIsPastDateAfterRelativeValidator;

        public DateInputIsPastDateAfterRelativeValidatorTests()
        {
            _dateInputIsPastDateAfterRelativeValidator = new(_mockRelativeDateHelper.Object);
            _mockRelativeDateHelper.Setup(_ => _.HasValidDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(true);
        }

        [Fact]
        public void Validate_ShouldCheck_TheElementTypePassedIsNotADateInputElement()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            // Act
            var result = _dateInputIsPastDateAfterRelativeValidator.Validate(element, null, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldRun_WhenDateIsOptional()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddDays(4);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate(It.IsAny<string>())).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.DAY,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .Build();

            element.Properties.IsPastDateAfterRelative = "3-d-ex";
            element.Properties.Optional = true;

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            _dateInputIsPastDateAfterRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            _mockRelativeDateHelper.Verify(_ => _.HasValidDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>()), Times.Once);
            _mockRelativeDateHelper.Verify(_ => _.GetRelativeDate(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenDayIsAfterRelativeDate()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddDays(-2);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("3-d-ex")).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.DAY,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("3-d-in")).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.DAY,
                Type = DateInputConstants.INCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-d-ex", "Date is too late")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-d-in", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());
            var inclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());

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
            var dateToCheck = DateTime.Today.AddDays(-3);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate(It.IsAny<string>())).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.DAY,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-d-ex", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateAfterRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenDayIsEqualRelativeDate_Inclusive()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddDays(-3);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate(It.IsAny<string>())).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.DAY,
                Type = DateInputConstants.INCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-d-in", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateAfterRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenDayIsAfterRelativeDate()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddDays(-4);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("3-d-ex")).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.DAY,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("3-d-in")).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.DAY,
                Type = DateInputConstants.INCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-d-ex", "Date is too late")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-d-in", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());
            var inclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());

            // Assert
            Assert.False(exclusiveResult.IsValid);
            Assert.Equal("Date is too late", exclusiveResult.Message);

            Assert.False(inclusiveResult.IsValid);
            Assert.Equal("Date is too late", inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenMonthIsAfterRelativeDate()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddDays(+0).AddMonths(-3);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("3-m-ex")).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.MONTH,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("3-m-in")).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.MONTH,
                Type = DateInputConstants.INCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-m-ex", "Date is too late")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-m-in", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());
            var inclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());

            // Assert
            Assert.False(exclusiveResult.IsValid);
            Assert.Equal("Date is too late", exclusiveResult.Message);

            Assert.True(inclusiveResult.IsValid);
            Assert.Equal(string.Empty, inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenMonthIsEqualRelativeDate_Exclusive()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddMonths(-3);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate(It.IsAny<string>())).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.MONTH,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-m-ex", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateAfterRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenMonthIsEqualRelativeDate_Inclusive()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddMonths(-3);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate(It.IsAny<string>())).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.MONTH,
                Type = DateInputConstants.INCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-m-in", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateAfterRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenMonthIsBeforeRelativeDate()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddMonths(-2);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("3-m-ex")).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.MONTH,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("3-m-in")).Returns(new RelativeDate()
            {
                Ammount = 3,
                Unit = DateInputConstants.MONTH,
                Type = DateInputConstants.INCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-m-ex", "Date is too late")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("3-m-in", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var exclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());
            var inclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());

            // Assert
            Assert.True(exclusiveResult.IsValid);
            Assert.Equal(string.Empty, exclusiveResult.Message);

            Assert.True(inclusiveResult.IsValid);
            Assert.Equal(string.Empty, inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenYearAfterRelativeDate()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddYears(-3);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("2-y-ex")).Returns(new RelativeDate()
            {
                Ammount = 2,
                Unit = DateInputConstants.YEAR,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("2-y-in")).Returns(new RelativeDate()
            {
                Ammount = 2,
                Unit = DateInputConstants.YEAR,
                Type = DateInputConstants.INCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("2-y-ex", "Date is too late")
                .Build();

            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("2-y-in", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var inclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());
            var exclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());

            // Assert
            Assert.False(inclusiveResult.IsValid);
            Assert.Equal("Date is too late", inclusiveResult.Message);

            Assert.False(exclusiveResult.IsValid);
            Assert.Equal("Date is too late", exclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsBeforeRelativeDate()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddYears(-1);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("2-y-ex")).Returns(new RelativeDate()
            {
                Ammount = 2,
                Unit = DateInputConstants.YEAR,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate("2-y-in")).Returns(new RelativeDate()
            {
                Ammount = 2,
                Unit = DateInputConstants.YEAR,
                Type = DateInputConstants.INCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var exclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("2-y-ex", "Date is too late")
                .Build();

            var inclusiveElement = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("2-y-in", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var inclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(inclusiveElement, viewModel, new FormSchema());
            var exclusiveResult = _dateInputIsPastDateAfterRelativeValidator.Validate(exclusiveElement, viewModel, new FormSchema());

            // Assert
            Assert.True(exclusiveResult.IsValid);
            Assert.Equal(string.Empty, exclusiveResult.Message);

            Assert.True(inclusiveResult.IsValid);
            Assert.Equal(string.Empty, inclusiveResult.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessage_WhenYearIsEqualRelativeDate_Exclusive()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddYears(-2);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate(It.IsAny<string>())).Returns(new RelativeDate()
            {
                Ammount = 2,
                Unit = DateInputConstants.YEAR,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("2-y-ex", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateAfterRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Date is too late", result.Message);
        }

        [Fact]
        public void Validate_ShouldNotShowValidationMessage_WhenYearIsEqualRelativeDate_Inclusive()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddYears(-2);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate(It.IsAny<string>())).Returns(new RelativeDate()
            {
                Ammount = 2,
                Unit = DateInputConstants.YEAR,
                Type = DateInputConstants.INCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("2-y-in", "Date is too late")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateAfterRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }


        [Fact]
        public void Validate_ShouldShowDefaultValidationMessage_WhenValidationIsTriggeredAndNoValidationMessageIsSet()
        {
            // Arrange
            var dateToCheck = DateTime.Today.AddDays(-4);

            _mockRelativeDateHelper.Setup(_ => _.GetRelativeDate(It.IsAny<string>())).Returns(new RelativeDate()
            {
                Ammount = 2,
                Unit = DateInputConstants.DAY,
                Type = DateInputConstants.EXCLUSIVE
            });

            _mockRelativeDateHelper.Setup(_ => _.GetChosenDate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>())).Returns(dateToCheck);

            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithIsPastDateAfterRelative("2-d-ex", "")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-date-day", dateToCheck.Day},
                {"test-date-month", dateToCheck.Month},
                {"test-date-year", dateToCheck.Year}
            };

            // Act
            var result = _dateInputIsPastDateAfterRelativeValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }
    }
}
