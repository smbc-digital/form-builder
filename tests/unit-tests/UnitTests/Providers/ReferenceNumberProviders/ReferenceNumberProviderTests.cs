using System;
using System.Threading.Tasks;
using Amazon.S3;
using form_builder.Configuration;
using form_builder.Gateways;
using form_builder.Providers.ReferenceNumbers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.ReferenceNumberProviders
{
    public class ReferenceNumberProviderTests
    {
        [Fact]
        public void ReferenceNumberProvider_Returns_Correct_Length_Reference()
        {
            // Arrange
            var referenceNumberProvider = new ReferenceNumberProvider();
            
            // Act
            var result = referenceNumberProvider.GetReference("TEST", 8);

            // Assert
            Assert.Equal(result.Length, 12);
        }

                [Fact]
        public void ReferenceNumberProvider_Returns_Reference_ThatStartsWith_Prefix()
        {
            // Arrange
            var referenceNumberProvider = new ReferenceNumberProvider();
            
            // Act
            var result = referenceNumberProvider.GetReference("TEST", 8);

            // Assert
            Assert.True(result.StartsWith("TEST"));
        }
    }
}