using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;
using Elements = form_builder.Models.Elements;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Element
{
    public class AddAnotherElementCheckTests
    {
        private readonly AddAnotherElementCheck _integrityCheck = new();

        [Fact]
        public void ValidateAddAnotherElement_NullProperties_ShouldAddError()
        {
            // Arrange
            var element = new Elements.Element() { Type = EElementType.AddAnother };

            // Act
            var result = _integrityCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Count.Equals(1));
            Assert.Collection(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void ValidateAddAnotherElement_NullElements_ShouldAddError()
        {
            // Arrange
            var element = new Elements.Element()
            {
                Type = EElementType.AddAnother,
                Properties = new BaseProperty()
                {
                    Elements = null
                }
            };

            // Act
            var result = _integrityCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Count.Equals(1));
            Assert.Collection(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}
