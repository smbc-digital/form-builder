using form_builder.Enum;
using form_builder_tests.Builders;
using System.Collections.Generic;
using form_builder.Validators;
using Xunit;
using System;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictFutureDatepickerValidatorTests
    {
        private readonly RestrictFutureDatepickerValidator _restrictFutureDateValidator = new RestrictFutureDatepickerValidator();

        [Fact]
        public void Validate_ShouldCheckRestrictFutureDatePropertyIsNotSet()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .Build();

            //Assert
            var result = _restrictFutureDateValidator.Validate(element, null);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ReturnsTrueWhenOptionalFieldsAreEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithRestrictFutureDate(true)
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            //Assert
            var result = _restrictFutureDateValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenFieldsAreEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictFutureDate(true)
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date", string.Empty);

            //Assert
            var result = _restrictFutureDateValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenFutureDateEntered()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictFutureDate(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            var tomorrow = DateTime.Today.AddDays(1);
            
            viewModel.Add("test-date", tomorrow.ToString("yyyy-MM-dd"));
          
            //Assert
            var result = _restrictFutureDateValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnTrueWhenPastDateIsNotEntered()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictFutureDate(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            var yesterday = DateTime.Today.AddDays(-1);

            viewModel.Add("test-date", yesterday.ToString("yyyy-MM-dd"));

            //Assert
            var result = _restrictFutureDateValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }
    }
}
