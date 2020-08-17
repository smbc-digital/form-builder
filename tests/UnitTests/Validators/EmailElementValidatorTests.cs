using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
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

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("email", "notanemail");

            //Assert
            var result = _emailElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the enter your email and try again", result.Message);
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

            var viewModel = new Dictionary<string, dynamic>();
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

            var viewModel = new Dictionary<string, dynamic>();

            //Assert
            var result = _requiredElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the enter your email and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowNoValidationMessageWhenEmailFieldIsEmptyAndOptional()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("email")
                .WithLabel("Enter your email")
                .WithEmail(true)
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("email", "");

            //Assert
            var result = _emailElementValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }
    }
}
