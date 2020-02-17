using form_builder.Enum;
using form_builder.Validators;
using form_builder_tests.Builders;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictFutureDateValidatorTests
    {
        private readonly RestrictFutureDateValidator _restrictFutureDateValidator = new RestrictFutureDateValidator();

        [Fact]
        public void Validate_ShouldCheckRestrictFutureDatePropertyIsNotSet()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
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
                .WithType(EElementType.DateInput)
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
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithRestrictFutureDate(true)
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

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
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithRestrictFutureDate(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date-day", "01");
            viewModel.Add("test-date-month", "01");
            viewModel.Add("test-date-year", "2030");

            //Assert
            var result = _restrictFutureDateValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnTrueWhenFutureDateIsNotEntered()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithRestrictFutureDate(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>()
            {
                { "test-date-day", "10"},
                { "test-date-month", "10" },
                { "test-date-year", "2010" }
            };
           

            //Assert
            var result = _restrictFutureDateValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }
    }
}
