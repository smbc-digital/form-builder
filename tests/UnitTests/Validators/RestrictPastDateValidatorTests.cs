using form_builder.Enum;
using form_builder.Validators;
using form_builder_tests.Builders;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictPastDateValidatorTests
    {
        
        private readonly RestrictPastDateValidator _restrictPastDateValidator = new RestrictPastDateValidator();

        [Fact]
        public void Validate_ShouldCheckRestrictPastDatePropertyIsNotSet()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .Build();

            //Assert
            var result = _restrictPastDateValidator.Validate(element, null);
            Assert.True(result.IsValid);
        }
        [Fact]
        public void Validate_ReturnsTrueWhenOptionalFieldsAreEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithRestrictPastDate(true)
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, string>();

            //Assert
            var result = _restrictPastDateValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenFieldsAreEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithRestrictPastDate(true)
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, string>();

            //Assert
            var result = _restrictPastDateValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowValidationMessageWhenPastDateEntered()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithRestrictPastDate(true)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-date-day", "01");
            viewModel.Add("test-date-month", "01");
            viewModel.Add("test-date-year", "2010");

            //Assert
            var result = _restrictPastDateValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Past date not allowed. Please enter a valid date", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnTrueWhenPastDateIsNotEntered()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithRestrictPastDate(true)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-date-day", "10");
            viewModel.Add("test-date-month", "10");
            viewModel.Add("test-date-year", "2030");

            //Assert
            var result = _restrictPastDateValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }
    }
}

