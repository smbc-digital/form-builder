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

        [Theory]
        [InlineData("fileone.txt", "fileon.txt", 10)]
        [InlineData("reallylongfilenameshouldbetrimmed.jpeg", "reall.jpeg", 10)]
        [InlineData("reallylongfilenameshouldbetrimmed.docx", "reallylong.docx", 15)]
        public void ToMaxSpecifiedStringLengthForFileName_ShouldLimitString_ToSuppliedLength(string value, string expected, int length)
        {
            // Act & Assert
            var result = value.ToMaxSpecifiedStringLengthForFileName(length);
            Assert.Equal(expected, result);
            Assert.Equal(length, result.Length);
        }

        [Theory]
        [InlineData("fileon.txt")]
        [InlineData("fileon.jpg")]
        [InlineData("file.txt")]
        [InlineData("f.doc")]
        public void ToMaxSpecifiedStringLengthForFileName_ShouldReturn_OriginalString_IfLength_IsBelowLimit(string value)
        {
            // Act & Assert
            var result = value.ToMaxSpecifiedStringLengthForFileName(10);
            Assert.Equal(value, result);
            Assert.True(result.Length <= 10);
        }

        [Fact]
        public void ToBookingRequestedMonthUrl_ShouldReturnValidUrl()
        {
            var formName = "test-form";
            // Act & Assert
            var result = formName.ToBookingRequestedMonthUrl("pageone");
            Assert.Equal("/booking/test-form/pageone/month", result);
        }
    }
}