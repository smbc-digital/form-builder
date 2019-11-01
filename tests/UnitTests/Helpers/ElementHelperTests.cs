using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class ElementHelperTests
    {
        private ElementHelper _elementHelper = new ElementHelper();

        //public ElementHelperTests(ElementHelper elementHelper)
        //{
        //    _elementHelper = elementHelper;
        //}

        [Fact]
        public void CurrentValue_ReturnsCurrentValueOfElement()
        {
            // Arrange
            var elements = new List<Element>
            {
                new Element
                   {
                        Type = EElementType.Textbox,
                        Properties = new Property
                        {
                             QuestionId = "test-id",
                             Label = "test-text",
                             Value = "this is the value"
                        }
                    }
            };
            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-id", "this is the value");
            // Act
            var result = _elementHelper.CurrentValue(elements[0], viewModel);

            // Assert
            Assert.Equal("this is the value", result);
        }

        [Fact]
        public void CurrentValue_ReturnsNoValueOfElement()
        {
            // Arrange
            var elements = new List<Element>
            {
                new Element
                   {
                        Type = EElementType.Textbox,
                        Properties = new Property
                        {
                             QuestionId = "test-id",
                             Label = "test-text",
                             Value = "this is the value"
                        }
                    }
            };
            var viewModel = new Dictionary<string, string>();
            viewModel.Add("test-id2", "this is the value");
            // Act
            var result = _elementHelper.CurrentValue(elements[0], viewModel);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void CheckForLabel_ReturnsTrue_IfLabelExists()
        {
            // Arrange
            var elements = new List<Element>
            {
                new Element
                   {
                        Type = EElementType.Textbox,
                        Properties = new Property
                        {
                             QuestionId = "test-id",
                             Label = "test-text"
                        }
                    }
            };
            var viewModel = new Dictionary<string, string>();

            // Act
            var result = _elementHelper.CheckForLabel(elements[0], viewModel);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckForLabel_ThrowsException_IfLabelDoesNotExists()
        {
            // Arrange
            var elements = new List<Element>
            {
                new Element
                   {
                        Type = EElementType.Textbox,
                        Properties = new Property
                        {
                             QuestionId = "test-id"
                        }
                    }
            };

            var viewModel = new Dictionary<string, string>();

            // Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForLabel(elements[0], viewModel));
        }

        [Fact]
        public void CheckForLabel_ThrowsException_IfLabelIsEmpty()
        {
            // Arrange
            var elements = new List<Element>
            {
                new Element
                   {
                        Type = EElementType.Textbox,
                        Properties = new Property
                        {
                             QuestionId = "test-id",
                             Label = string.Empty
                        }
                    }
            };

            var viewModel = new Dictionary<string, string>();

            // Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForLabel(elements[0], viewModel));
        }
    }
}
