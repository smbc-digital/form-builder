using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models.Elements;
using form_builder.Validators;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Models
{
    public class ElementTests
    {
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private readonly Mock<IElementValidator> _testValidator = new Mock<IElementValidator>();

        public ElementTests()
        {
            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Returns(new ValidationResult { IsValid = false });

            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());
        }

        [Fact]
        public void DisplayAriaDescribedby_ShouldReturnTrue_WhenPropertiesHint_NotEmpty()
        {
            var element = new ElementBuilder()
                            .WithHint("non-empty")
                            .Build();

            Assert.True(element.DisplayAriaDescribedby);
        }

        [Fact]
        public void DisplayAriaDescribedby_ShouldReturnFalse_WhenPropertiesHint_IsEmpty()
        {
            var element = new ElementBuilder()
                            .WithHint(string.Empty)
                            .Build();

            Assert.False(element.DisplayAriaDescribedby);
        }

        [Theory]
        [InlineData(EElementType.Textarea)]
        [InlineData(EElementType.Textbox)]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_Textbox_And_TextArea(EElementType type)
        {
            var questionId = "test-question-id";
            var value = "test-value";
            var length = 20;

            var element = new ElementBuilder()
                            .WithType(type)
                            .WithQuestionId(questionId)
                            .WithValue(value)
                            .WithMaxLength(length)
                            .Build();

            var result = element.GenerateElementProperties();

            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("name"));
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsKey("maxlength"));
            Assert.True(result.ContainsKey("value"));
            Assert.True(result.ContainsValue(questionId));
            Assert.True(result.ContainsValue(value));
            Assert.True(result.ContainsValue(length));
        }

        [Theory]
        [InlineData(200, "5")]
        [InlineData(500, "5")]
        [InlineData(501, "15")]
        [InlineData(2000, "15")]
        public void GenerateElementProperties_ShouldReturnCorrectRowsSize_When_MaxLengthSupllied(int length, string expectedValue)
        {
            var questionId = "test-question-id";
            var value = "test-value";

            var element = new ElementBuilder()
                            .WithType(EElementType.Textarea)
                            .WithQuestionId(questionId)
                            .WithValue(value)
                            .WithMaxLength(length)
                            .Build();

            var result = element.GenerateElementProperties();

            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("rows"));
            Assert.True(result.ContainsValue(expectedValue));
        }

        [Theory]
        [InlineData(50)]
        [InlineData(199)]
        public void GenerateElementProperties_ShouldReturn_NoRowsSize_When_MaxLengthSupllied_Below200(int length)
        {
            var questionId = "test-question-id";
            var value = "test-value";

            var element = new ElementBuilder()
                            .WithType(EElementType.Textarea)
                            .WithQuestionId(questionId)
                            .WithValue(value)
                            .WithMaxLength(length)
                            .Build();

            var result = element.GenerateElementProperties();

            Assert.NotEmpty(result);
            Assert.False(result.ContainsKey("rows"));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectRowsSize_When_UsingDefaultMaxLength()
        {
            var questionId = "test-question-id";
            var value = "test-value";

            var element = new ElementBuilder()
                            .WithType(EElementType.Textarea)
                            .WithQuestionId(questionId)
                            .WithValue(value)
                            .Build();

            var result = element.GenerateElementProperties();

            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("rows"));
            Assert.True(result.ContainsValue("5"));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_Address()
        {
            var questionId = "test--address-question-id";

            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .Build();

            var result = element.GenerateElementProperties();

            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsValue($"{questionId}-postcode"));
        }
        
        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_AddressSelect()
        {
            var questionId = "test--address-question-id";

            var element = (form_builder.Models.Elements.Address)new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .Build();

            var result = element.GenerateElementProperties("Select");

            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsKey("name"));
            Assert.True(result.ContainsValue(element.AddressSearchQuestionId));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_Street()
        {
            var questionId = "test-street-question-id";
            var length = 5;

            var element = (form_builder.Models.Elements.Street) new ElementBuilder()
                            .WithType(EElementType.Street)
                            .WithQuestionId(questionId)
                            .WithMaxLength(length)
                            .Build();

            var result = element.GenerateElementProperties();

            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsKey("maxlength"));
            Assert.True(result.ContainsValue(element.StreetSearchQuestionId));
            Assert.True(result.ContainsValue(length));
        }

        [Fact]
        public void DescribeValue_ShouldReturn_OnlyHint()
        {
            var questionId = "test-question-id";

            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .WithHint("hint")
                            .Build();

            var result = element.GetDescribedByAttributeValue();

            Assert.NotNull(result);
            Assert.Equal(element.HintId, result);
        }

        [Fact]
        public void DescribeValue_ShouldReturn_OnlyError()
        {
            var questionId = "test-question-id";

            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(questionId, "test");

            element.Validate(viewModel, _validators.Object);
            var result = element.GetDescribedByAttributeValue();

            Assert.NotNull(result);
            Assert.Equal(element.ErrorId, result);
        }

        [Fact]
        public void DescribeValue_ShouldReturn_Both_ErrorAndHint()
        {
            var questionId = "test-question-id";

            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .WithHint("hint")
                            .Build();
                            
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(questionId, "test");

            element.Validate(viewModel, _validators.Object);
            var result = element.GetDescribedByAttributeValue();

            Assert.NotNull(result);
            Assert.Contains(element.HintId, result);
            Assert.Contains(element.QuestionId, result);
        }
    }
}
