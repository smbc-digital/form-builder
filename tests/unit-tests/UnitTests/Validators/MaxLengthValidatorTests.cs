using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class MaxLengthValidatorTests
    {
        readonly MaxLengthValidator _validator = new MaxLengthValidator();

        [Fact]
        public void IfMaxLengthExceededValidationShouldBeFalse()
        {
            var element = new ElementBuilder()
                .WithQuestionId("test-id")
                .WithLabel("Label")
                .WithNumeric(false)
                .WithMaxLength(20)
                .Build();


            var viewModel = new Dictionary<string, dynamic>() { { "test-id", "123456789012345678901234567890" } };

            var result = _validator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Label has a maximum length of 20", result.Message);
        }

        [Fact]
        public void IfMaxLengthNotExceededValidationShouldBeTrue()
        {
            var element = new ElementBuilder()
                .WithQuestionId("test-id")
                .WithLabel("Label")
                .WithNumeric(false)
                .WithMaxLength(20)
                .Build();

            var viewModel = new Dictionary<string, dynamic>() { { "test-id", "1234567" } };

            var result = _validator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }
    }
}
