using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class TimeInputElementValidatorTests
    {
        private readonly TimeInputValidator _dateInputElementValidator = new TimeInputValidator();

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
                .WithType(EElementType.TimeInput)
                .WithQuestionId("test-time")
                .WithLabel("Time")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            //Assert
            var result = _dateInputElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the time and try again", result.Message);
        }

        [Fact]
       public void Validate_ShouldCheckTimeIsNotValid()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.TimeInput)
                .WithQuestionId("test-time")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-time-hours", "aa");
            viewModel.Add("test-time-minutes", "aa");
            viewModel.Add("test-time-ampm", "aaaa");

            //Assert
            var result = _dateInputElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the time and try again", result.Message);
        }
    }
}
