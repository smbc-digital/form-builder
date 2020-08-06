using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class AutomaticAddressElementValidatorTests
    {
        private readonly AutomaticAddressElementValidator _automaticAddressElementValidator = new AutomaticAddressElementValidator();

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDoesNotContainerAddressKey()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _automaticAddressElementValidator.Validate(element, viewModel);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldValidateUPRN_WhenKeySupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-address", "234567434567");

            // Act
            var result = _automaticAddressElementValidator.Validate(element, viewModel);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_WhenInvalidUPRN()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("testaddress")
                .WithType(EElementType.Address)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("testaddress-address", "566");

            // Act
            var result = _automaticAddressElementValidator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}
