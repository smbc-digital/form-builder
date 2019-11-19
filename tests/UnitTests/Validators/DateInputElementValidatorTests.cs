using form_builder.Enum;
using form_builder.Validators;
using form_builder_tests.Builders;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class DateInputElementValidatorTests
    {
        private readonly DateInputElementValidator _dateInputElementValidator = new DateInputElementValidator();

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
        public void Validate_ShouldShowValidationMessageWhenFieldsAreEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, string>();

            //Assert
            var result = _dateInputElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Date is required", result.Message);
        }

        [Fact]
       public void Validate_ShouldCheckDateIsNotValid()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-date-day", "aa");
            viewModel.Add("test-date-month", "aa");
            viewModel.Add("test-date-year", "aaaa");

            //Assert
            var result = _dateInputElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the date and try again", result.Message);
        }
    }
}
