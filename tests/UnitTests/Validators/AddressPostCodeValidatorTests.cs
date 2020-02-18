using form_builder.Enum;
using form_builder.Validators;
using form_builder_tests.Builders;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class AddressPostCodeValidatorTests
    {

        private readonly AddressPostcodeValidator _addressPostcodeValidator = new AddressPostcodeValidator();
        [Fact]
        public void Validate_ShouldReturnTrue_WhenDoesNotPostcode()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            //Assert
            var result = _addressPostcodeValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }


        [Fact]
        public void Validate_ShouldValidatePostcode_WhenPostcodeSupplied()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-postcode", "SK4 1AA");

            //Assert
            var result = _addressPostcodeValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldNotValidatePostcode_WhenInvalidPostcodeSupplied()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-postcode", "Elephant");

            //Assert
            var result = _addressPostcodeValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
        }
    }
}
