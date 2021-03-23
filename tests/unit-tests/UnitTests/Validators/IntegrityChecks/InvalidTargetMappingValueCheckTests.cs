using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class InvalidTargetMappingValueCheckTests
    {
        [Theory]
        [InlineData("invalid-tagretMapping")]
        [InlineData("tagret4")]
        [InlineData("target$")]
        [InlineData("target.")]
        [InlineData(".target")]
        public void TargetMappingValue_NotValid(string targetMapping)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithTargetMapping(targetMapping)
                .WithType(EElementType.Textarea)
                .Build();

            // Act
            InvalidTargetMappingValueCheck check = new();
            var result = check.Validate(element);
            
            // Assert
            Assert.False(result.IsValid);
            Assert.Collection(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Theory]
        [InlineData("validtargetMapping")]
        [InlineData("valid.target")]
        [InlineData("valid.target.mapping")]
        public void TargetMappingValue_Valid(string targetMapping)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithTargetMapping(targetMapping)
                .Build();

            // Act
            InvalidTargetMappingValueCheck check = new();
            var result = check.Validate(element);
            
            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}