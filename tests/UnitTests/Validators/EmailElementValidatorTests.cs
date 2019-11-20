using form_builder.Enum;
using form_builder.Validators;
using form_builder_tests.Builders;
using System.Collections.Generic;
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
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("email")
                .WithLabel("Enter your email")
                .WithEmail(true)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("email", "notanemail");

            //Assert
            var result = _emailElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Enter your email must be a valid email address", result.Message);
        }

        [Fact]
        public void Validate_ShouldCheckEmailIsValid()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("email")
                .WithLabel("Enter your email")
                .WithEmail(true)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("email", "isanemail@stockport.gov.uk");

            //Assert
            var result = _emailElementValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldShowNormalValidationMessageWhenEmailFieldIsEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("email")
                .WithLabel("Enter your email")
                .WithEmail(true)
                .Build();

            var viewModel = new Dictionary<string, string>();

            //Assert
            var result = _requiredElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Enter your email is required", result.Message);
        }
    }
}
