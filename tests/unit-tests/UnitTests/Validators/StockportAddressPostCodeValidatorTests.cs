using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class StockportAddressPostCodeValidatorTests
    {
        private readonly StockportAddressPostcodeElementValidator _stockportPostcodeValidator = new StockportAddressPostcodeElementValidator();
        
        [Fact]
        public void Validate_ShouldReturnTrue_WhenDoesNotPostcode()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _stockportPostcodeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }


        [Fact]
        public void Validate_ShouldValidatePostcode_WhenPostcodeSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-postcode", "SK4 1AA");

            // Act
            var result = _stockportPostcodeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldNotValidatePostcode_WhenInvalidPostcodeSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .WithStockportPostcode(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic> {{"testaddress-postcode", "Elephant"}};

            // Act
            var result = _stockportPostcodeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldNotValidatePostcode_WhenNonStockportPostcodeSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .WithStockportPostcode(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-postcode", "OL16 0AE");

            // Act
            var result = _stockportPostcodeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
        }
    }
}