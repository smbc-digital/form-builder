using System.Text.RegularExpressions;
using form_builder.Providers.ReferenceNumbers;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.ReferenceNumberProviders
{
    public class ReferenceNumberProviderTests
    {
        ReferenceNumberProvider _referenceNumberProvider;
        const string prefix = "TEST";

        public ReferenceNumberProviderTests() => _referenceNumberProvider = new ReferenceNumberProvider();

        [Fact]
        public void ReferenceNumberProvider_Returns_Reference_ThatStartsWith_Prefix()
        {
            // Arrange
            const string localPrefix = "RNP";

            // Act
            string result = _referenceNumberProvider.GetReference(localPrefix);

            // Assert
            Assert.True(result.StartsWith(localPrefix));
        }

        [Fact]
        public void ReferenceNumberProvider_Returns_Correct_Length_Reference()
        {
            // Act
            string result = _referenceNumberProvider.GetReference(prefix, length: 8);

            // Assert
            Assert.Equal(result.Length, 12);
        }

        [Fact]
        public void ReferenceNumberProvider_Returns_CorrectCase()
        {
            // Arrange
            Regex regex = new Regex("^[A-Z0-9]*$");

            // Act
            string result = _referenceNumberProvider.GetReference(prefix, caseSensitive: false);

            // Assert
            Assert.True(regex.IsMatch(result));
        }
    }
}