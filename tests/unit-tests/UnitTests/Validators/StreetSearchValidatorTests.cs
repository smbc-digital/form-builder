﻿using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class StreetSearchValidatorTests
    {

        private readonly StreetSearchValidator _streetSearchValidator = new StreetSearchValidator();

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDoesNotStreetSearch()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Street)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _streetSearchValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldValidateStreet_WhenStreetSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("teststreet")
                .WithType(EElementType.Street)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("teststreet", "Some Street");

            // Act
            var result = _streetSearchValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldNotValidateStreet_WhenInvalidStreetSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("teststreet")
                .WithType(EElementType.Street)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("teststreet", "Some_Street");

            // Act
            var result = _streetSearchValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(ValidationConstants.STREET_INCORRECT_FORMAT, result.Message);
        }

        [Fact]
        public void Validate_ShouldNotValidateStreet_WhenIncorrectLengthStreetSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("teststreet")
                .WithType(EElementType.Street)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("teststreet", "So");

            // Act
            var result = _streetSearchValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(ValidationConstants.STREET_INCORRECT_LENGTH, result.Message);
        }

        [Fact]
        public void Validate_ShouldNotValidateStreet_WhenIncorrectLengtStreetSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("teststreet")
                .WithType(EElementType.Street)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("teststreet", "So");

            // Act
            var result = _streetSearchValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Street name must be 3 characters or more", result.Message);
        }
    }
}
