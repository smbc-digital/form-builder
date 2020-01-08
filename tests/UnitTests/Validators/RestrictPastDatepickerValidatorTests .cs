using form_builder.Enum;
using form_builder_tests.Builders;
using System.Collections.Generic;
using form_builder.Validators;
using Xunit;
using System;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictPastDatepickerValidatorTests
    {
        private readonly RestrictPastDatepickerValidator _restrictPastDatepickerValidator = new RestrictPastDatepickerValidator();

        [Fact]
        public void Validate_ShouldCheckRestrictCurrentDatePropertyIsNotSet()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .Build();

            //Assert
            var result = _restrictPastDatepickerValidator.Validate(element, null);
            Assert.True(result.IsValid);
        }
        [Fact]
        public void Validate_ReturnsTrueWhenOptionalFieldsAreEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithRestrictPastDate(true)
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, string>();

            //Assert
            var result = _restrictPastDatepickerValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenFieldsAreEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictPastDate(true)
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, string>();

            //Assert
            var result = _restrictPastDatepickerValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenPastDateEntered()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictPastDate(true)
                .Build();

            var viewModel = new Dictionary<string, string>();
            var yesterday = DateTime.Today.AddDays(-1);
            
            viewModel.Add("test-date", yesterday.ToString("yyyy-MM-dd"));
          
            //Assert
            var result = _restrictPastDatepickerValidator.Validate(element, viewModel);
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
                .WithRestrictPastDate(true)
                .Build();

            var viewModel = new Dictionary<string, string>();
            var tomorrow = DateTime.Today.AddDays(1);

            viewModel.Add("test-date", tomorrow.ToString("yyyy-MM-dd"));

            //Assert
            var result = _restrictPastDatepickerValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }
    }
}
