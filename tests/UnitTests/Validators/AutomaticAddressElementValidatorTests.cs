using form_builder.Enum;
using form_builder.Validators;
using form_builder_tests.Builders;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class AutomaticAddressElementValidatorTests
    {
        private readonly AutomaticAddressElementValidator _automaticAddressElementValidator = new AutomaticAddressElementValidator();

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDoesNotContainerAddressKey()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            //Assert
            var result = _automaticAddressElementValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldValidateUPRN_WhenKeySupplied()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-address", "234567434567");

            //Assert
            var result = _automaticAddressElementValidator.Validate(element, viewModel);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_WhenInvalidUPRN()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-address", "566");

            //Assert
            var result = _automaticAddressElementValidator.Validate(element, viewModel);
            Assert.False(result.IsValid);
        }
    }
}
