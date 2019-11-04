using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder_tests.Builders;
using System;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class ElementHelperTests
    {
        private ElementHelper _elementHelper = new ElementHelper();

        [Fact]
        public void CurrentValue_ReturnsCurrentValueOfElement()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
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

        [Fact]
        public void CheckForLabel_ReturnsTrue_IfLabelExists()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Textbox)
               .WithQuestionId("test-id")
               .WithLabel("test-text")
               .Build();
           
            var viewModel = new Dictionary<string, string>();

            // Act
            var result = _elementHelper.CheckForLabel(element, viewModel);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckForLabel_ThrowsException_IfLabelDoesNotExists()
        {
            // Arrange
            var element = new ElementBuilder()
              .WithType(EElementType.Textbox)
              .WithQuestionId("test-id")
              .Build();

            var viewModel = new Dictionary<string, string>();

            // Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForLabel(element, viewModel));
        }

        [Fact]
        public void CheckForLabel_ThrowsException_IfLabelIsEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
              .WithType(EElementType.Textbox)
              .WithQuestionId("test-id")
              .WithLabel(string.Empty)
              .Build();

            var viewModel = new Dictionary<string, string>();

            // Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForLabel(element, viewModel));
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
       
    }
}
