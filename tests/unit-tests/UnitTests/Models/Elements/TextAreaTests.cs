using form_builder.Builders;
using form_builder.Enum;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class TextAreaTests
    {

        [Theory]
        [InlineData(50, "5")]
        [InlineData(200, "5")]
        [InlineData(500, "5")]
        [InlineData(501, "15")]
        [InlineData(2000, "15")]
        [InlineData(4000, "15")]
        public void GenerateElementProperties_ShouldReturnCorrectRowsSize_When_MaxLengthSupllied(int length, string expectedValue)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("test-question-id")
                .WithValue("test-value")
                .WithMaxLength(length)
                .Build();

            // Act
            var result = element.GenerateElementProperties();

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("rows"));
            Assert.True(result.ContainsValue(expectedValue));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturn_DefaultRows_Value_WhenMaxLength_NotSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("test-question-id")
                .WithValue("test-value")
                .Build();

            // Act
            var result = element.GenerateElementProperties();

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("rows"));
            Assert.True(result.ContainsValue("5"));
        }

    }
}