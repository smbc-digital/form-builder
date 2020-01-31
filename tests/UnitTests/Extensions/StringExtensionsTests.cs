using form_builder.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class StringExtensionsTests
    {

        [Theory]
        [InlineData("ui-test", "Int")]
        [InlineData("local", "local")]
        [InlineData("int", "Int")]
        [InlineData("qa", "QA")]
        [InlineData("stage", "Staging")]
        [InlineData("prod", "Prod")]
        public void ToS3EnvPrefix_ShouldReturnCorrectValueForEnv(string env, string expectedResult)
        {
            var testString = env;

            Assert.Equal(expectedResult, testString.ToS3EnvPrefix());
        }

        [Fact(Skip="no longer throwing an error, returning a default of local")]
        public void ToS3EnvPrefix_ShouldThrowException_WhenUnknownEnv()
        {
            var testString = "unkown-env";

            Assert.Throws<Exception>(() => testString.ToS3EnvPrefix());
        }
    }
}
