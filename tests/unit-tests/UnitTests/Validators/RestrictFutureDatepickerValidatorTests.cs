using System;
using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictFutureDatepickerValidatorTests
    {
        private readonly RestrictFutureDatepickerValidator _restrictFutureDateValidator = new RestrictFutureDatepickerValidator();

        [Fact]
        public void Validate_ShouldCheckRestrictFutureDatePropertyIsNotSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .Build();

            // Act
            var result = _restrictFutureDateValidator.Validate(element, null);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ReturnsTrueWhenOptionalFieldsAreEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithRestrictFutureDate(true)
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _restrictFutureDateValidator.Validate(element, viewModel);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenFieldsAreEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictFutureDate(true)
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, dynamic> { { "test-date", string.Empty } };

            // Act
            var result = _restrictFutureDateValidator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenFutureDateEntered()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictFutureDate(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            var tomorrow = DateTime.Today.AddDays(1);
            viewModel.Add("test-date", tomorrow.ToString("yyyy-MM-dd"));

            // Act
            var result = _restrictFutureDateValidator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnTrueWhenPastDateIsNotEntered()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictFutureDate(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            var yesterday = DateTime.Today.AddDays(-1);
            viewModel.Add("test-date", yesterday.ToString("yyyy-MM-dd"));

            // Act
            var result = _restrictFutureDateValidator.Validate(element, viewModel);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}