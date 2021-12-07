using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class MinMaxValidatorTests
    {
        private readonly MinMaxValidator _validator = new MinMaxValidator();

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_WhenValueGreaterThanMax()
        {
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithNumeric(true)
                .WithLabel(label)
                .WithMax("20")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "123");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal($"{label} must be less than or equal to 20", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_WhenValueLessThanMin()
        {
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithNumeric(true)
                .WithLabel(label)
                .WithMin("20")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "12");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal($"{label} must be greater than or equal to 20", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_WhenOutOfRange()
        {
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithNumeric(true)
                .WithLabel(label)
                .WithMax("20")
                .WithMin("10")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "7");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal($"{label} must be between 10 and 20 inclusive", result.Message);
        }
    }
}
