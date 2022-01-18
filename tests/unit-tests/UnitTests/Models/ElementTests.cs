using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Validators;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models
{
    public class ElementTests
    {
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new();
        private readonly Mock<IElementValidator> _testValidator = new();

        public ElementTests()
        {
            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = false });

            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());
        }

        [Fact]
        public void DisplayAriaDescribedby_ShouldReturnTrue_WhenPropertiesHint_NotEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithHint("non-empty")
                .Build();

            // Assert
            Assert.True(element.DisplayAriaDescribedby);
        }

        [Fact]
        public void DisplayAriaDescribedby_ShouldReturnFalse_WhenPropertiesHint_IsEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithHint(string.Empty)
                .Build();

            // Assert
            Assert.False(element.DisplayAriaDescribedby);
        }
        
        [Fact]
        public void DisplayAriaDescribedby_ShouldReturnFalse_WhenPropertiesWarning_IsEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithWarning(string.Empty)
                .Build();

            // Assert
            Assert.False(element.DisplayAriaDescribedby);
        }

        [Fact]
        public void DisplayAriaDescribedby_ShouldReturnTrue_WhenPropertiesWarning_NotEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithWarning("non-empty")
                .Build();

            // Assert
            Assert.True(element.DisplayAriaDescribedby);
        }

        [Theory]
        [InlineData(EElementType.Textarea)]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_TextArea(EElementType type)
        {
            // Arrange
            var questionId = "test-question-id";
            var value = "test-value";

            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId(questionId)
                .WithValue(value)
                .Build();

            // Act
            var result = element.GenerateElementProperties();

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("name"));
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsKey("value"));
            Assert.True(result.ContainsValue(questionId));
            Assert.True(result.ContainsValue(value));
        }

        [Theory]
        [InlineData(EElementType.Textbox)]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_Textbox(EElementType type)
        {
            // Arrange
            var questionId = "test-question-id";
            var value = "test-value";
            var length = 205;

            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId(questionId)
                .WithValue(value)
                .WithMaxLength(length)
                .Build();

            // Act
            var result = element.GenerateElementProperties();

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("name"));
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsKey("maxlength"));
            Assert.True(result.ContainsKey("value"));
            Assert.True(result.ContainsValue(questionId));
            Assert.True(result.ContainsValue(value));
            Assert.True(result.ContainsValue(length));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectRowsSize_When_UsingDefaultMaxLength()
        {
            // Arrange
            var questionId = "test-question-id";
            var value = "test-value";

            var element = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId(questionId)
                .WithValue(value)
                .Build();

            // Act
            var result = element.GenerateElementProperties();

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("rows"));
            Assert.True(result.ContainsValue("5"));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_Address()
        {
            // Arrange
            var questionId = "test--address-question-id";
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithQuestionId(questionId)
                .Build();

            // Act
            var result = element.GenerateElementProperties();

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsValue($"{questionId}-postcode"));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_AddressSelect()
        {
            // Arrange
            var questionId = "test--address-question-id";
            var element = (Address)new ElementBuilder()
                .WithType(EElementType.Address)
                .WithQuestionId(questionId)
                .Build();

            // Act
            var result = element.GenerateElementProperties("Select");

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsKey("name"));
            Assert.True(result.ContainsValue(element.AddressSearchQuestionId));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_Street()
        {
            // Arrange
            var questionId = "test-street-question-id";
            var length = 5;

            var element = (Street)new ElementBuilder()
                            .WithType(EElementType.Street)
                            .WithQuestionId(questionId)
                            .WithMaxLength(length)
                            .Build();

            // Act
            var result = element.GenerateElementProperties();

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsKey("maxlength"));
            Assert.True(result.ContainsValue(element.StreetSearchQuestionId));
            Assert.True(result.ContainsValue(length));
        }

        [Fact]
        public void DescribeValue_ShouldReturn_OnlyHint()
        {
            // Arrange
            var questionId = "test-question-id";
            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .WithHint("hint")
                            .Build();

            // Act
            var result = element.GetDescribedByAttributeValue();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(element.HintId, result);
        }

        [Fact]
        public void DescribeValue_ShouldReturn_OnlyError()
        {
            // Arrange
            var questionId = "test-question-id";
            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .Build();

            var viewModel = new Dictionary<string, dynamic> { { questionId, "test" } };
            element.Validate(viewModel, _validators.Object, new FormSchema());

            // Act
            var result = element.GetDescribedByAttributeValue();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(element.ErrorId, result);
        }

        [Fact]
        public void DescribeValue_ShouldReturn_Both_ErrorAndHint()
        {
            // Arrange
            var questionId = "test-question-id";
            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .WithHint("hint")
                            .Build();

            var viewModel = new Dictionary<string, dynamic> { { questionId, "test" } };
            element.Validate(viewModel, _validators.Object, new FormSchema());

            // Act
            var result = element.GetDescribedByAttributeValue();

            // Assert
            Assert.NotNull(result);
            Assert.Contains(element.HintId, result);
            Assert.Contains(element.QuestionId, result);
        }
    }
}