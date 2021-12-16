using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class DateInputWithInRangeValidatorTests
    {
        private readonly DateInputWithInRangeValidator _dateInputWithInRangeValidator = new DateInputWithInRangeValidator();

        [Fact]
        public void Validate_ShouldCheckTheElementTypeIsNotDateInput()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            // Act
            var result = _dateInputWithInRangeValidator.Validate(element, null, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_WhenYearNotWithinRange()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test")
                .Build();
            element.Properties.WithinRange = "18-Y";

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-day", "01");
            viewModel.Add("test-month", "01");
            viewModel.Add("test-year", "2002");

            // Act
            var result = _dateInputWithInRangeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);

        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenYearWithinRange()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test")
                .Build();
            element.Properties.WithinRange = "18-Y";

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-day", "01");
            viewModel.Add("test-month", "01");
            viewModel.Add("test-year", "2020");

            // Act
            var result = _dateInputWithInRangeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
