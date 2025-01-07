using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class DateInputOutsideRangeValidatorTests
    {
        private readonly DateInputOutsideRangeValidator _dateInputOutsideRangeValidator = new DateInputOutsideRangeValidator();

        [Fact]
        public void Validate_ShouldCheckTheElementTypeIsNotDateInput()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            // Act
            var result = _dateInputOutsideRangeValidator.Validate(element, null, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldCheckDateIsOptional()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .Build();

            element.Properties.OutsideRange = "1";
            element.Properties.Optional = true;

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _dateInputOutsideRangeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_WhenYearNotOutsideRange()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test")
                .Build();
            element.Properties.OutsideRange = "18-y";

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-day", "01");
            viewModel.Add("test-month", "01");
            viewModel.Add("test-year", "2010");

            // Act
            var result = _dateInputOutsideRangeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);

        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenYearOutsideRange()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test")
                .Build();
            element.Properties.OutsideRange = "18";

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-day", "01");
            viewModel.Add("test-month", "01");
            viewModel.Add("test-year", "2001");

            // Act
            var result = _dateInputOutsideRangeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
