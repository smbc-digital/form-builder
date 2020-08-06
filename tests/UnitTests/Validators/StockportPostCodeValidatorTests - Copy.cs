using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class StockportPostCodeValidatorTests
    {

        private readonly StockportPostcodeElementValidator _stockportPostcodeValidator = new StockportPostcodeElementValidator();

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDoesNotPostcode()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("postcode")
                .WithType(EElementType.Textbox)
                .Build();

            var viewModel = new Dictionary<string, dynamic> {{"postcode", "OL16 0AE"}};

            // Act
            var result = _stockportPostcodeValidator.Validate(element, viewModel);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldValidatePostcode_WhenPostcodeSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("postcode")
                .WithType(EElementType.Textbox)
                .WithStockportPostcode(true)                 
                .Build();

            var viewModel = new Dictionary<string, dynamic> {{"postcode", "SK4 1AA"}};

            // Act
            var result = _stockportPostcodeValidator.Validate(element, viewModel);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldNotValidatePostcode_WhenInvalidPostcodeSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("postcode")
                .WithType(EElementType.Textbox)
                .WithStockportPostcode(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic> {{"postcode", "Elephant"}};

            // Act
            var result = _stockportPostcodeValidator.Validate(element, viewModel);
            
            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldNotValidatePostcode_WhenNonStockportPostcodeSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("postcode")
                .WithType(EElementType.Textbox)
                .WithStockportPostcode(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic> {{"postcode", "OL16 0AE"}};

            // Act
            var result = _stockportPostcodeValidator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}