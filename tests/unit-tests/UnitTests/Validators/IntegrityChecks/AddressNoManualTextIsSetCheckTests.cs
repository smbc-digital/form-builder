using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class AddressNoManualTextIsSetCheckTests
    {
        [Fact]
        public void AddressNoManualTextIsSetCheck_IsValid_If_DetailsTextIsSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithDisableManualAddress(true)
                .WithNoManualAddressDetailText("Test")
                .Build();

            // Act
            AddressNoManualTextIsSetCheck check = new();

            // Act
            var result = check.Validate(element);
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }

        [Fact]
        public void CheckAddressNoManualTextIsSet_ShouldThrowException_IfNoDetailsTextIsSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithDisableManualAddress(true)
                .Build();

            // Act
            AddressNoManualTextIsSetCheck check = new();

            // Assert
            var result = check.Validate(element);
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}