using System;
using form_builder.Extensions;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class TypeExtensionsTests
    {
        [Theory]
        [InlineData("", "String")]
        [InlineData(1, "Int")]
        [InlineData(true, "Boolean")]
        public void ConvertTypeToFormattedString_ShouldReturnCorrectResult(object obj, string expectedResult)
        {
            // Arrange
            var type = obj.GetType();

            // Act
            var result = type.ConvertTypeToFormattedString();

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertTypeToFormattedString_ShouldReturnCorrectResult_ForDateTime()
        {
            // Arrange
            var type = DateTime.Now.GetType();

            // Act
            var result = type.ConvertTypeToFormattedString();

            // Assert
            Assert.Equal("DateTime", result);
        }

        [Fact]
        public void ConvertTypeToFormattedString_ShouldReturnCorrectResult_ForGuid()
        {
            // Arrange
            var type = Guid.NewGuid().GetType();

            // Act
            var result = type.ConvertTypeToFormattedString();

            // Assert
            Assert.Equal("Guid", result);
        }

        [Fact]
        public void ConvertTypeToFormattedString_ShouldReturnCorrectResult_ForNullableBool()
        {
            // Arrange
            var instance = new ClassWithNullableBoolean();
            var props = instance.GetType().GetProperties();

            // Act
            var result = props[0].PropertyType.ConvertTypeToFormattedString();

            // Assert
            Assert.Equal("Nullable[Boolean]", result);
        }
    }

    public class ClassWithNullableBoolean
    {
        public bool? NullableBoolean { get; set; }
    }
}
