using System;
using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class DateInputElementValidatorTests
    {
        private readonly DateInputElementValidator _dateInputElementValidator = new DateInputElementValidator();

        [Fact]
        public void Validate_ShouldCheckTheElementTypeIsNotDateInput()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            // Act
            var result = _dateInputElementValidator.Validate(element, null, new form_builder.Models.FormSchema());

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
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

       [Fact]
       public void Validate_ShouldCheckDateIsNotValid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date-day", "aa");
            viewModel.Add("test-date-month", "aa");
            viewModel.Add("test-date-year", "aaaa");

            // Act
            var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

       [Fact]
       public void Validate_ShouldReturnFalse_WhenYearIsGreaterThanMax()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test")
                .Build();

            var maxYear = DateTime.Now.Year + 100;
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-day", "12");
            viewModel.Add("test-month", "12");
            viewModel.Add("test-year", "2920");

            // Act
            var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal($"Year must be less than or equal to { maxYear}", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_WhenYearIsGreaterThanMax_AndIsOptional()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithOptional(true)
                .WithQuestionId("test")
                .Build();

            var maxYear = DateTime.Now.Year + 100;
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-day", "12");
            viewModel.Add("test-month", "12");
            viewModel.Add("test-year", "2920");

            // Act
            var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal($"Year must be less than or equal to { maxYear}", result.Message);
        }
    }
}
