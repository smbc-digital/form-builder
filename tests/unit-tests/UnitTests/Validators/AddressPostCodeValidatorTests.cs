using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class AddressPostCodeValidatorTests
    {
        private readonly AddressPostcodeValidator _addressPostcodeValidator = new AddressPostcodeValidator();

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDoesNotPostcode()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _addressPostcodeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

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
            var result = _addressPostcodeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

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
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-postcode", "Elephant");

            // Act
            var result = _addressPostcodeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldValidate_NonStandardPostcode_WhenValidatePostcode_Is_False()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithValidatePostcode(false)
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-postcode", "90210");

            // Act
            var result = _addressPostcodeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldNotValidate_NonStandardPostcode_WhenValidatePostcode_Is_True()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithValidatePostcode(true)
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-postcode", "90210");

            // Act
            var result = _addressPostcodeValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
        }
    }
}
