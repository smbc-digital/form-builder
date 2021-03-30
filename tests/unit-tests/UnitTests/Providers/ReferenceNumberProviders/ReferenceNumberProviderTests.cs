using form_builder.Configuration;
using form_builder.Providers.ReferenceNumbers;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.ReferenceNumberProviders
{
    public class ReferenceNumberProviderTests
    {
        private readonly ReferenceNumberProvider _referenceNumberProvider;
        private readonly Mock<IOptions<FormConfiguration>> _mockFormConfiguration = new();

        private const string PREFIX = "TEST";

        public ReferenceNumberProviderTests() 
        {
            _mockFormConfiguration
                .Setup(options => options.Value)
                .Returns(new FormConfiguration{
                    ValidReferenceCharacters = "abc123456789"
                });

            _referenceNumberProvider = new ReferenceNumberProvider(_mockFormConfiguration.Object);
        }

        [Fact]
        public void ReferenceNumberProvider_Returns_Reference_ThatStartsWith_Prefix()
        {
            // Arrange
            const string localPrefix = "RNP";

            // Act
            string result = _referenceNumberProvider.GetReference(localPrefix);

            // Assert
            Assert.StartsWith(localPrefix, result);
        }

        [Fact]
        public void ReferenceNumberProvider_Returns_Correct_Length_Reference()
        {
            // Act
            string result = _referenceNumberProvider.GetReference(PREFIX, length: 8);

            // Assert
            Assert.Equal(12, result.Length);
        }
    }
}