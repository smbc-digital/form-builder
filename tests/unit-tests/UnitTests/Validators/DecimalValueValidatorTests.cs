using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Models;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class DecimalValueValidatorTests
    {
        private readonly DecimalValueValidator _validator = new DecimalValueValidator();

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_When_Decimal_Property_Not_True()
        {
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithDecimal(false)
                .Build();

            var result = _validator.Validate(element, null, new FormSchema());

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("15")]
        [InlineData("1.3")]
        [InlineData("1.35")]
        [InlineData("5000")]
        [InlineData("5000.55")]
        [InlineData("5000.1")]
        [InlineData("5,000")]
        [InlineData("5,000.45")]
        [InlineData("5,000.1")]
        public void Validate_ShouldReturn_True_ValidationResult_When_Valid_Decimal(string value)
        {
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithDecimal(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", value);

            var result = _validator.Validate(element, viewModel, new FormSchema());

            Assert.True(result.IsValid);
            Assert.Equal(string.Empty, result.Message);
        }

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_When__Not_Valid_Decimal()
        {
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithDecimal(true)
                .WithLabel(label)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "a123");

            var result = _validator.Validate(element, viewModel, new FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal($"{label} must be a valid number", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturn_CustomValidationResult_WhenElement_NotValid_Decimal()
        {
            var errorMessage = "Provide an valid number";
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithDecimal(true)
                .WithLabel(label)
                .WithNotAnDecimalValidationMessage(errorMessage)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "a123");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal(errorMessage, result.Message);
        }
    }
}
