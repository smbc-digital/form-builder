using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class ExactNumberOptionsCheckboxValidatorTests
    {

        private readonly ExactNumberOptionsCheckboxValidator _numberRequiredCheckboxValidator = new ExactNumberOptionsCheckboxValidator();

        [Theory]
        [InlineData(EElementType.DateInput)]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void Validate_ShouldReturnTrue_WhenElementIsNot_Checkbox(EElementType type)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .Build();

            // Act
            var result = _numberRequiredCheckboxValidator.Validate(element, null, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }


        [Fact]
        public void Validate_ShouldReturnTrue_WhenNoAnswerProvided()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId("testElement")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _numberRequiredCheckboxValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenExactNumberOptionsProvided()
        {
            // Arrange
            var questionId = "testElement";
            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId(questionId)
                .WithExactNumberOptions(2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(questionId, "testAnswer1,testanswer2");

            // Act
            var result = _numberRequiredCheckboxValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }


        [Fact]
        public void Validate_ShouldReturnFalse_WhenLessThanTheExactNumberOptionsAreChecked()
        {
            // Arrange

            var questionId = "testElement";
            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId(questionId)
                .WithExactNumberOptions(2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(questionId, "testAnswer1");

            // Act
            var result = _numberRequiredCheckboxValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_WhenMoreThanTheExactNumberOptionsAreChecked()
        {
            // Arrange

            var questionId = "testElement";
            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId(questionId)
                .WithExactNumberOptions(2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(questionId, "testAnswer1,testAnswer2,testAnswer3");

            // Act
            var result = _numberRequiredCheckboxValidator.Validate(element, viewModel, new FormSchema());

            // Assert
            Assert.False(result.IsValid);
        }
    }
}
