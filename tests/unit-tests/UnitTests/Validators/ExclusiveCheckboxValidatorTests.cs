using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class ExclusiveCheckboxValidatorTests
    {

        private readonly ExclusiveCheckboxValidator _exclusiveCheckboxValidator = new ExclusiveCheckboxValidator();

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
            var result = _exclusiveCheckboxValidator.Validate(element, null, new form_builder.Models.FormSchema());

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
            var result = _exclusiveCheckboxValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenSingleAnswerProvided()
        {
            // Arrange
            var questionId = "testElement";
            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId(questionId)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(questionId, "testAnswer");

            // Act
            var result = _exclusiveCheckboxValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenExclusiveOptionNotProvided()
        {
            // Arrange
            var questionId = "testElement";
            var options = new List<Option>();

            options.Add(new Option());
            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId(questionId)
                .WithOptions(options)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(questionId, "testAnswer, testAnswerTwo");

            // Act
            var result = _exclusiveCheckboxValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_WhenExclusiveAndNonExclusiveOptionIsChecked()
        {
            // Arrange
            var questionId = "testElement";
            var options = new List<Option>();

            var option1 = new Option();
            option1.Value = "Option1";
            option1.Exclusive = true;
            option1.Checked = true;

            var option2 = new Option();
            option2.Value = "Option2";
            option2.Checked = true;

            options.Add(option1);
            options.Add(option2);
            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId(questionId)
                .WithOptions(options)
                .WithExclusiveCheckboxValidationMessage("ErrorMessage")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(questionId, "Option1, Option2");

            // Act
            var result = _exclusiveCheckboxValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("ErrorMessage", result.Message);
        }
    }
}
