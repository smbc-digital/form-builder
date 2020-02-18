using form_builder.Enum;
using form_builder.Validators;
using form_builder_tests.Builders;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class RegexValidatorTests
    {
        private readonly RegexElementValidator  _regexElementValidator = new RegexElementValidator();
        private readonly RequiredElementValidator _requiredElementValidator = new RequiredElementValidator();

        [Fact]
        public void Validate_ShouldShowRegexValidationMessageWhenNiFieldIsEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("ni")
                .WithLabel("NI Number")
                .WithRegex("^[A-Za-z]{2}[0-9]{6}[A-Za-z]{1}$")                
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("ni", "");

            //Assert
            var result = _regexElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the NI Number and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldCheckRegexIsValid()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("ni")
                .WithLabel("NI Number")
                .WithRegex("^[A-Za-z]{2}[0-9]{6}[A-Za-z]{1}$")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("ni", "BV123456E");

            //Assert
            var result = _regexElementValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldCheckRegexIsInvalid()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("ni")
                .WithLabel("NI Number")
                .WithRegex("^[A-Za-z]{2}[0-9]{6}[A-Za-z]{1}$")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("ni", "notAnNiNumber");

            //Assert
            var result = _regexElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldShowNormalRegexMessageWhenNIFieldIsEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("niNumber")
                .WithLabel("NI Number")
                .WithRegex("^[A-Za-z]{2}[0-9]{6}[A-Za-z]{1}$")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            //Assert
            var result = _requiredElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
            Assert.Equal("Check the ni number and try again", result.Message);
        }

        [Fact]
        public void Validate_ShouldShowNoRegexMessageWhenNIFieldIsEmptyAndOptional()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("ni")
                .WithLabel("NI Number")
                .WithOptional(true)
                .WithRegex("^[A-Za-z]{2}[0-9]{6}[A-Za-z]{1}$")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("ni", "");

            //Assert
            var result = _regexElementValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }
    }
}
