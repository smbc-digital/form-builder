using System;
using form_builder.Extensions;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("uitest", "Int")]
        [InlineData("local", "local")]
        [InlineData("int", "Int")]
        [InlineData("qa", "QA")]
        [InlineData("stage", "Stage")]
        [InlineData("prod", "Prod")]
        public void ToS3EnvPrefix_ShouldReturnCorrectValueForEnv(string env, string expectedResult)
        {
            // Arrange
            var testString = env;

            // Act & Assert
            Assert.Equal(expectedResult, testString.ToS3EnvPrefix());
        }

        [Fact]
        public void ToS3EnvPrefix_ShouldThrowException_WhenUnknownEnv()
        {
            // Arrange
            var testString = "unknown-env";

            // Act & Assert
            Assert.Throws<Exception>(() => testString.ToS3EnvPrefix());
        }
    }
}