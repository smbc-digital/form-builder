﻿using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictCurrentDateValidatorTests
    {
        private readonly RestrictCurrentDateValidator _restrictCurrentDateValidator = new RestrictCurrentDateValidator();

        [Fact]
        public void Validate_ShouldCheckRestrictCurrentDatePropertyIsNotSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .Build();

            // Act
            var result = _restrictCurrentDateValidator.Validate(element, null, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }
        [Fact]
        public void Validate_ReturnsTrueWhenOptionalFieldsAreEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithRestrictCurrentDate(true)
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _restrictCurrentDateValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

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
                .WithRestrictCurrentDate(true)
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _restrictCurrentDateValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenCurrentDateEntered()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithRestrictCurrentDate(true, "Current Date Validation Message")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            var today = DateTime.Today;
            viewModel.Add("test-date-year", today.Year.ToString());
            viewModel.Add("test-date-month", today.Month.ToString());
            viewModel.Add("test-date-day", today.Day.ToString());

            // Act
            var result = _restrictCurrentDateValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Current Date Validation Message", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnTrueWhenCurrentDateIsNotEntered()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictCurrentDate(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date-day", "10");
            viewModel.Add("test-date-month", "10");
            viewModel.Add("test-date-year", "2012");

            // Act
            var result = _restrictCurrentDateValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
