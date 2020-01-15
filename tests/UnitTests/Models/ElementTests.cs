using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Validators;
using form_builder_tests.Builders;
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
            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
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
        [InlineData(50, "small")]
        [InlineData(199, "small")]
        [InlineData(501, "large")]
        [InlineData(2000, "large")]
        public void GenerateElementProperties_ShouldReturnCorrectClassValue_When_MaxLengthSupllied(int length, string expectedClassname)
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
            Assert.True(result.ContainsKey("class"));
            Assert.True(result.ContainsValue(expectedClassname));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectClassValue_When_UsingDefaultMaxLength()
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
            Assert.True(result.ContainsKey("class"));
            Assert.True(result.ContainsValue("small"));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_Address()
        {
            var questionId = "test--address-question-id";
            var length = 5;

            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .WithMaxLength(length)
                            .Build();

            var result = element.GenerateElementProperties();

            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsKey("maxlength"));
            Assert.True(result.ContainsValue($"{questionId}-postcode"));
            Assert.True(result.ContainsValue(length));
        }

        [Fact]
        public void GenerateElementProperties_ShouldReturnCorrectPropertiesFor_Street()
        {
            var questionId = "test-street-question-id";
            var length = 5;

            var element = new ElementBuilder()
                            .WithType(EElementType.Street)
                            .WithQuestionId(questionId)
                            .WithMaxLength(length)
                            .Build();

            var result = element.GenerateElementProperties();

            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsKey("maxlength"));
            Assert.True(result.ContainsValue($"{questionId}-street"));
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

            var result = element.DescribedByValue();

            Assert.NotNull(result);
            Assert.Equal($"{questionId}-hint", result);
        }

        [Fact]
        public void DescribeValue_ShouldReturn_OnlyError()
        {
            var questionId = "test-question-id";

            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add(questionId, "test");

            element.Validate(viewModel, _validators.Object);
            var result = element.DescribedByValue();

            Assert.NotNull(result);
            Assert.Equal($"{questionId}-error", result);
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
            var viewModel = new Dictionary<string, string>();
            viewModel.Add(questionId, "test");

            element.Validate(viewModel, _validators.Object);
            var result = element.DescribedByValue();

            Assert.NotNull(result);
            Assert.Contains($"{questionId}-hint", result);
            Assert.Contains($"{questionId}-error", result);
        }

        [Fact]
        public void GetAllQuestionIds_ShouldReturnArrayWithThreeItems_WhenElementHasThreeInputs()
        {
            var questionId = "date";

            var element = new ElementBuilder()
                            .WithType(EElementType.DateInput)
                            .WithQuestionId(questionId)
                            .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add(questionId + "-day", "daytest");
            viewModel.Add(questionId + "-month", "monthtest");
            viewModel.Add(questionId + "-year", "yeartest");

            var result = element.GetAllQuestionIds(element, viewModel);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);

            Assert.Equal("date-day", result[0].Properties.QuestionId);
            Assert.Equal("date-month", result[1].Properties.QuestionId);
            Assert.Equal("date-year", result[2].Properties.QuestionId);

            Assert.Equal("daytest", result[0].Properties.Value);
            Assert.Equal("monthtest", result[1].Properties.Value);
            Assert.Equal("yeartest", result[2].Properties.Value);
        }
    }
}
