using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class EmailElementValidatorTests
    {
        private readonly EmailElementValidator _emailElementValidator = new EmailElementValidator();
        private readonly RequiredElementValidator _requiredElementValidator = new RequiredElementValidator();

        [Fact]
        public void Validate_ShouldShowEmailValidationMessageWhenEmailFieldIsEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("email")
                .WithLabel("Enter your email")
                .WithEmail(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("email", "notanemail");

            // Act
            var result = _emailElementValidator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Enter an email address in the correct format, like name@example.com", result.Message);
        }

        [Fact]
        public void Validate_ShouldCheckEmailIsValid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("email")
                .WithLabel("Enter your email")
                .WithEmail(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("email", "isanemail@stockport.gov.uk");

            // Act
            var result = _emailElementValidator.Validate(element, viewModel);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldShowNormalValidationMessageWhenEmailFieldIsEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("email")
                .WithLabel("Enter your email")
                .WithEmail(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _requiredElementValidator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Enter the enter your email", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowNoValidationMessageWhenEmailFieldIsEmptyAndOptional()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("email")
                .WithLabel("Enter your email")
                .WithEmail(true)
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("email", "");

            // Act
            var result = _emailElementValidator.Validate(element, viewModel);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
