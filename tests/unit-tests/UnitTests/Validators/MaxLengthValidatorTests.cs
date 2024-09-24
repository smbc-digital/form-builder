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

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());
            Assert.False(result.IsValid);
            Assert.Equal("Shorten the text so it's no longer than 20 characters", result.Message);
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

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_WhenGreater_Than_MaxLength()
        {
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("test-id")
                .WithNumeric(true)
                .WithLabel(label)
                .WithMaxLength(7)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-id", "12345678");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal($"{label} must be 7 digits or less", result.Message);
        }
    }
}
