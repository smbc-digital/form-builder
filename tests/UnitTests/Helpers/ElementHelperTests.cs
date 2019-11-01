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
    }
}
