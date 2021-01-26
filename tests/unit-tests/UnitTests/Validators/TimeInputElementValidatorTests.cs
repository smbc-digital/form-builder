using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class TimeInputElementValidatorTests
    {
        private readonly TimeInputValidator _dateInputElementValidator = new TimeInputValidator();

        [Fact]
        public void Validate_ShouldCheckTheElementTypeIsNotDateInput()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            // Assert
            var result = _dateInputElementValidator.Validate(element, null, new form_builder.Models.FormSchema());
            Assert.True(result.IsValid);
        }
        [Fact]
        public void Validate_ShouldShowValidationMessageWhenFieldsAreEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.TimeInput)
                .WithQuestionId("test-time")
                .WithLabel("Time")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the time and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldCheckTimeIsNotValid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.TimeInput)
                .WithQuestionId("test-time")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {$"test-time{TimeConstants.HOURS_SUFFIX}", "aa"},
                {$"test-time{TimeConstants.MINUTES_SUFFIX}s", "aa"},
                {$"test-time{TimeConstants.AM_PM_SUFFIX}", "aaaa"}
            };

            // Act
            var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Check the time and try again", result.Message);
        }
    }
}