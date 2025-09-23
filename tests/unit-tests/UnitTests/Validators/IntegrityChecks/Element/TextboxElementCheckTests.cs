using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Element
{
    public class TextboxElementCheckTests
    {
        private readonly TextboxElementCheck _integrityCheck = new();

        [Fact]
        public void Validate_ShouldReturn_True_When_Valid_NumericElement()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("numeric")
                .WithNumeric(true)
                .Build();

            // Act & Assert
            var result = _integrityCheck.Validate(element);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_True_When_Valid_DecimalElement()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("decimal")
                .WithDecimal(true)
                .Build();

            // Act & Assert
            var result = _integrityCheck.Validate(element);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_False_When__Numeric_Decimal_Properties_Both_Set_True()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("mixed")
                .WithDecimal(true)
                .WithNumeric(true)
                .Build();

            // Act & Assert
            var result = _integrityCheck.Validate(element);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_False_When__MinLength_Property_Less_Than_Zero()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("lessThanZero")
                .WithMinLength(-1)
                .Build();

            // Act & Assert
            var result = _integrityCheck.Validate(element);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_False_When__MinLength_Property_Is_Zero_And_Not_Optional()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("lessThanZero")
                .WithMinLength(0)
                .WithOptional(false)
                .Build();

            // Act & Assert
            var result = _integrityCheck.Validate(element);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_False_When__MinLength_Property_Is_Not_Null_And_Optional()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("lessThanZero")
                .WithMinLength(5)
                .WithOptional(true)
                .Build();

            // Act & Assert
            var result = _integrityCheck.Validate(element);

            Assert.False(result.IsValid);
        }
    }
}
