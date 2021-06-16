using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform.UserSchema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class AddAnotherPageTransformFactoryTests
    {
        private readonly Mock<IPageHelper> _mockPageHelper = new();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new();
        private readonly AddAnotherPageTransformFactory _transformFactory;
        private readonly Page _page;

        public AddAnotherPageTransformFactoryTests()
        {
            var textboxElement = new ElementBuilder()
                .WithQuestionId("textbox")
                .WithType(EElementType.Textbox)
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithLabel("Person")
                .WithQuestionId("person")
                .WithType(EElementType.AddAnother)
                .WithNestedElement(textboxElement)
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { textboxElement };

            _page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(addAnotherElement)
                .Build();

            var savedAnswers = new FormAnswers
            {
                FormData = new Dictionary<string, object>
                {
                    { "addAnotherFieldset-person", 1 }
                }
            };

            _mockPageHelper
                .Setup(_ => _.GetSavedAnswers(It.IsAny<string>()))
                .Returns(savedAnswers);

            _transformFactory = new AddAnotherPageTransformFactory(_mockPageHelper.Object, _mockSessionHelper.Object);
        }

        [Fact]
        public void Transform_ShouldReturnCorrectNumberOfElementsOnPage()
        {
            // Act
            var result = _transformFactory.Transform(_page, "guid");

            // Assert
            Assert.Equal(12, result.Elements.Count);
        }

        [Fact]
        public void Transform_ShouldRetainOriginalAddnotherElement()
        {
            // Act
            var result = _transformFactory.Transform(_page, "guid");

            // Assert
            Assert.Single(result.Elements.Where(_ => _.Type.Equals(EElementType.AddAnother)));
        }

        [Fact]
        public void Transform_ShouldReturnCorrectNumberOf_FieldsetElements()
        {
            // Act
            var result = _transformFactory.Transform(_page, "guid");
            var openingFieldsets = result.Elements.Count(_ => _.Type.Equals(EElementType.Fieldset) && _.Properties.OpeningTag);
            var closingFieldsets = result.Elements.Count(_ => _.Type.Equals(EElementType.Fieldset) && !_.Properties.OpeningTag);

            // Assert
            Assert.Equal(4, result.Elements.Count(_ => _.Type.Equals(EElementType.Fieldset)));
            Assert.Equal(2, openingFieldsets);
            Assert.Equal(2, closingFieldsets);
        }

        [Fact]
        public void Transform_ShouldReturnCorrectNumberOf_LegendElements()
        {
            // Act
            var result = _transformFactory.Transform(_page, "guid");

            // Assert
            Assert.Equal(2, result.Elements.Count(_ => _.Type.Equals(EElementType.Legend)));
        }

        [Fact]
        public void Transform_ShouldReturnCorrect_ButtonElements()
        {
            // Act
            var result = _transformFactory.Transform(_page, "guid");
            var removeButtonZero = result.Elements.Where(_ => _.Type.Equals(EElementType.Button) && _.Properties.ButtonName.Equals("remove-0"));
            var removeButtonOne = result.Elements.Where(_ => _.Type.Equals(EElementType.Button) && _.Properties.ButtonName.Equals("remove-1"));
            var addAnotherButton = result.Elements.Where(_ => _.Type.Equals(EElementType.Button) && _.Properties.ButtonName.Equals("addAnotherFieldset"));

            // Assert
            Assert.Equal(3, result.Elements.Count(_ => _.Type.Equals(EElementType.Button)));
            Assert.Single(removeButtonZero);
            Assert.Single(removeButtonOne);
            Assert.Single(addAnotherButton);
        }

        [Fact]
        public void Transform_ShouldReturnCorrect_TextboxElements()
        {
            // Act
            var result = _transformFactory.Transform(_page, "guid");
            var textboxZero = result.Elements.Where(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox:0:")).ToList();
            var textboxOne = result.Elements.Where(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox:1:")).ToList();

            // Assert
            Assert.Equal(2, result.Elements.Count(_ => _.Type.Equals(EElementType.Textbox)));
            Assert.Single(textboxZero);
            Assert.Single(textboxOne);
            Assert.True(textboxZero[0].Properties.IsDynamicallyGeneratedElement);
            Assert.True(textboxOne[0].Properties.IsDynamicallyGeneratedElement);
        }
    }
}
