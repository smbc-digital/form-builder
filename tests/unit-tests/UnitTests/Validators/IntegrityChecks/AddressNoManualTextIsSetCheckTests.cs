using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
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

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Act
            var check = new AddressNoManualTextIsSetCheck();

            // Act
            var result = check.Validate(schema);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void CheckAddressNoManualTextIsSet_ShouldThrowException_IfNoDetailsTextIsSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address
                )
                .WithDisableManualAddress(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Act
            var check = new AddressNoManualTextIsSetCheck();

            // Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
        }
    }
}