using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder_tests.Builders;
using System;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class ElementHelperTests
    {
        private ElementHelper _elementHelper = new ElementHelper();

        [Theory]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void CurrentValue_ReturnsCurrentValueOfElement(EElementType elementType)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(elementType)
                .WithQuestionId("test-id")
                .WithLabel("test-text")
                .WithValue("this is the value")
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-id", "this is the value");

            // Act
            var result = _elementHelper.CurrentValue(element, viewModel);

            // Assert
            Assert.Equal("this is the value", result);
        }

        [Fact]
        public void CurrentValue_ReturnsNoValueOfElement()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("test-id")
                .WithLabel("test-text")
                .WithValue("this is the value")
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-id2", "this is the value");

            // Act
            var result = _elementHelper.CurrentValue(element, viewModel);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void CheckForLabel_ReturnsTrue_IfLabelExists(EElementType elementType)
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(elementType)
               .WithQuestionId("test-id")
               .WithLabel("test-text")
               .Build();
           
            var viewModel = new Dictionary<string, string>();

            // Act
            var result = _elementHelper.CheckForLabel(element, viewModel);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void CheckForLabel_ThrowsException_IfLabelDoesNotExists(EElementType elementType)
        {
            // Arrange
            var element = new ElementBuilder()
              .WithType(elementType)
              .WithQuestionId("test-id")
              .Build();

            var viewModel = new Dictionary<string, string>();

            // Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForLabel(element, viewModel));
        }

        [Theory]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void CheckForLabel_ThrowsException_IfLabelIsEmpty(EElementType elementType)
        {
            // Arrange
            var element = new ElementBuilder()
              .WithType(elementType)
              .WithQuestionId("test-id")
              .WithLabel(string.Empty)
              .Build();

            var viewModel = new Dictionary<string, string>();

            // Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForLabel(element, viewModel));
        }

        [Fact]
        public void CheckForMaxLength_ReturnsTrue_IfMaxLengthExists()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("issueOne")
               .WithMaxLength(2000)
               .Build();

            var viewModel = new Dictionary<string, string>();

            // Act
            var result = _elementHelper.CheckForMaxLength(element, viewModel);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckForMaxLength_ThrowsException_IfMaxLengthDoesNotExist()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("issueOne")
               .Build();

            var viewModel = new Dictionary<string, string>();

            // Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForMaxLength(element, viewModel));
        }

        [Fact]
        public void CheckForMaxLength_ReturnsTrue_IfMaxLengthExistsAndAboveZero()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("issueOne")
               .WithMaxLength(1)
               .Build();

            var viewModel = new Dictionary<string, string>();

            // Act
            var result = _elementHelper.CheckForMaxLength(element, viewModel);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckForMaxLength_ThrowsException_IfMaxLengthExistsAndIsZero()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("issueOne")
               .WithMaxLength(0)
               .Build();

            var viewModel = new Dictionary<string, string>();

            // Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForMaxLength(element, viewModel));
        }

        [Theory]
        [InlineData(EElementType.P, "paragraph")]
        [InlineData(EElementType.H1,"Header 1")]
        [InlineData(EElementType.H2, "Header 2")]
        [InlineData(EElementType.H3, "Header 3")]
        [InlineData(EElementType.H4, "Header 4")]
        [InlineData(EElementType.H5, "Header 5")]
        [InlineData(EElementType.H6, "Header 6")]
        public void CreateGenericHtmlElement(EElementType eElementType, string text)
        {
            var element = new ElementBuilder()
                .WithType(eElementType)
                .WithPropertyText(text)
                .Build();
           
            Assert.Equal(text, element.Properties.Text);
        }

        [Theory]
        [InlineData(EElementType.OL)]
        [InlineData(EElementType.UL)]
        public void CreateLists(EElementType eElementType)
        {
            List<string> listItems = new List<string>{ "item 1", "item 2", "item 3" };

            var element = new ElementBuilder()
                .WithType(eElementType)
                .WithListItems(listItems)
                .Build();

            Assert.Equal(3, element.Properties.ListItems.Count);
            Assert.Equal("item 1", element.Properties.ListItems[0]);
                                
        }

        [Fact]
        public void CreateImage()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.Img)
                .WithAltText("alt text")
                .WithSource("source")
                .Build();

            Assert.Equal("alt text", element.Properties.AltText);
            Assert.Equal("source", element.Properties.Source);


        }

        [Fact]
        public void CreateRadioButton()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("questionId")
                .WithLabel("Label")
                .WithOptions(new List<Option>
                { new Option { Value = "option1", Text = "Option 1", Hint = "Option 1 Hint" }, 
                  new Option { Value = "option2", Text = "Option 2", Hint = "Option 2 Hint" } })
                .Build();

            Assert.Equal("questionId", element.Properties.QuestionId);
            Assert.Equal("Label", element.Properties.Label);

            Assert.Equal("option1", element.Properties.Options[0].Value);
            Assert.Equal("Option 1", element.Properties.Options[0].Text);
            Assert.Equal("Option 1 Hint", element.Properties.Options[0].Hint);


            Assert.Equal("option2", element.Properties.Options[1].Value);
            Assert.Equal("Option 2", element.Properties.Options[1].Text);
            Assert.Equal("Option 2 Hint", element.Properties.Options[1].Hint);
        }
       
    }
}
