using form_builder.Enum;
using form_builder.Validators;
using form_builder_tests.Builders;
using System;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class DatePickerElementValidatorTests
    {
        private readonly DatePickerElementValidator _dateInputElementValidator = new DatePickerElementValidator();

        [Fact]
        public void Validate_ShouldCheckTheElementTypeIsNotDateInput()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            //Assert
            var result = _dateInputElementValidator.Validate(element, null);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldCheckDateIsNotValid()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-date", "2222aaaa");

            //Assert
            var result = _dateInputElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldCheckDateIsValid()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-date", "2019-08-02");

            //Assert
            var result = _dateInputElementValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldCheckDate_IsNotMoreThanOneHundredYears_InFuture_WhenNoMaxSet()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-date", "2230-01-01");

            //Assert
            var result = _dateInputElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal($"Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnCustomerValidationMessage_WhenDateIsTooFarInFuture()
        {
            //Arrange
            var upperLimitMessage = "customer upper limit";
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithMax("2025")
                .WithUpperLimitValidationMessage(upperLimitMessage)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-date", "2050-01-01");

            //Assert
            var result = _dateInputElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal(upperLimitMessage, result.Message);
        }
    }
}