using form_builder.Enum;
using form_builder_tests.Builders;
using System.Collections.Generic;
using form_builder.Validators;
using Xunit;
using System;
using form_builder.Builders;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictCurrentDatepickerValidatorTests
    {
        private readonly RestrictCurrentDatepickerValidator _restrictCurrentDateValidator = new RestrictCurrentDatepickerValidator();

        [Fact]
        public void Validate_ShouldCheckRestrictCurrentDatePropertyIsNotSet()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .Build();

            //Assert
            var result = _restrictCurrentDateValidator.Validate(element, null);
            Assert.True(result.IsValid);
        }
        [Fact]
        public void Validate_ReturnsTrueWhenOptionalFieldsAreEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithRestrictCurrentDate(true)
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            //Assert
            var result = _restrictCurrentDateValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenFieldsAreEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictCurrentDate(true)
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
             viewModel.Add("test-date", string.Empty);

            //Assert
            var result = _restrictCurrentDateValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenCurrentDateEntered()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictCurrentDate(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            var today = DateTime.Today;
            viewModel.Add("test-date", today.Day.ToString());

            //Assert
            var result = _restrictCurrentDateValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnTrueWhenCurrentDateIsNotEntered()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithRestrictCurrentDate(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date", DateTime.Today.AddDays(2).ToString("yyyy-MM-dd"));
           
            //Assert
            var result = _restrictCurrentDateValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }
    }
}
