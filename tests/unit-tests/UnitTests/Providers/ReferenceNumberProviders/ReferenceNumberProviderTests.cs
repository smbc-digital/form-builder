using form_builder.Providers.ReferenceNumbers;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.ReferenceNumberProviders
{
    public class ReferenceNumberProviderTests
    {
        ReferenceNumberProvider _referenceNumberProvider;

        public ReferenceNumberProviderTests()
        {
            _referenceNumberProvider = new ReferenceNumberProvider();
        }

        [Fact]
        public void ReferenceNumberProvider_Returns_Correct_Length_Reference()
        {          
            // Act
            var result = _referenceNumberProvider.GetReference("TEST", 8);

            // Assert
            Assert.Equal(result.Length, 12);
        }

        [Fact]
        public void ReferenceNumberProvider_Returns_Reference_ThatStartsWith_Prefix()
        {            
            // Act
            var result = _referenceNumberProvider.GetReference("TEST", 8);

            // Assert
            Assert.True(result.StartsWith("TEST"));
        }
    }
}