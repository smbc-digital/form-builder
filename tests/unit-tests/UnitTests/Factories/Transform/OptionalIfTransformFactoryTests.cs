using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Factories.Transform.UserSchema;
using form_builder.Models;
using form_builder.Models.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class OptionalIfTransformFactoryTests
    {
        private readonly OptionalIfTransformFactory _transformFactory;
        private readonly Page _page;

        private readonly FormAnswers _savedAnswers = new(){};

        public OptionalIfTransformFactoryTests()
        {
            var textboxElement = new ElementBuilder()
                .WithQuestionId("textbox")
                .WithType(EElementType.Textbox)
                .WithOptionalIf("optionalIf")
                .WithOptional(false)
                .Build();

            var textboxElement2 = new ElementBuilder()
                .WithQuestionId("textbox")
                .WithType(EElementType.Textbox)
                .WithOptionalIf("optionalIf")
                .Build();

            _page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(textboxElement)
                .Build();

            _transformFactory = new OptionalIfTransformFactory();
        }

        // TODOmikel - e.g. tests -
        // 1. Have an element that's required, pass it through the optionalIf, check that its now optional / not optional
        // 2. Repeat but with optionalNotIf
        // also check that the integrity checks work

        [Fact]
        public async Task Transform_ShouldReturnCorrectNumberOfElementsOnPage()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);

            // Assert
            Assert.Single(result.Elements);
        }

        [Fact]
        public async Task Transform_ShouldReturnCorrectNumberOf_LegendElements()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);

            // Assert
            Assert.Equal(2, result.Elements.Count(_ => _.Type.Equals(EElementType.Legend)));
        }

        [Fact]
        public async Task Transform_ShouldReturnCorrectNumberOf_FieldsetElements()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);
            var openingFieldsets = result.Elements.Count(_ => _.Type.Equals(EElementType.Fieldset) && _.Properties.OpeningTag);
            var closingFieldsets = result.Elements.Count(_ => _.Type.Equals(EElementType.Fieldset) && !_.Properties.OpeningTag);

            // Assert
            Assert.Equal(4, result.Elements.Count(_ => _.Type.Equals(EElementType.Fieldset)));
            Assert.Equal(2, openingFieldsets);
            Assert.Equal(2, closingFieldsets);
        }

        [Fact]
        public async Task Transform_ShouldReturnCorrect_ButtonElements()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);
            var removeButtonZero = result.Elements.Where(_ => _.Type.Equals(EElementType.Button) && _.Properties.ButtonName.Equals("remove-1"));
            var removeButtonOne = result.Elements.Where(_ => _.Type.Equals(EElementType.Button) && _.Properties.ButtonName.Equals("remove-2"));
            var addAnotherButton = result.Elements.Where(_ => _.Type.Equals(EElementType.Button) && _.Properties.ButtonName.Equals(AddAnotherConstants.AddAnotherButtonKey)).ToList();

            // Assert
            Assert.Equal(3, result.Elements.Count(_ => _.Type.Equals(EElementType.Button)));
            Assert.Single(removeButtonZero);
            Assert.Single(removeButtonOne);
            Assert.Single(addAnotherButton);
            Assert.Equal("Add another person", addAnotherButton[0].Properties.Text);
        }

        [Fact]
        public async Task Transform_ShouldNotReturnAddAnotherButtonElement_WhenMaximumFieldsetsReached()
        {
            // Act
            var textboxElement = new ElementBuilder()
                .WithQuestionId("textbox")
                .WithType(EElementType.Textbox)
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithLabel("Person")
                .WithQuestionId("person")
                .WithType(EElementType.AddAnother)
                .WithNestedElement(textboxElement)
                .WithMaximumFieldsets(2)
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { textboxElement };

            var page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(addAnotherElement)
                .Build();

            var result = await _transformFactory.Transform(page, _savedAnswers);
            var addAnotherButton = result.Elements.Where(_ => _.Type.Equals(EElementType.Button) && _.Properties.ButtonName.Equals(AddAnotherConstants.AddAnotherButtonKey)).ToList();

            // Assert
            Assert.Equal(2, result.Elements.Count(_ => _.Type.Equals(EElementType.Button)));
            Assert.Empty(addAnotherButton);
        }

        [Fact]
        public async Task Transform_ShouldReturnCorrect_TextboxElements()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);
            var textboxZero = result.Elements.Where(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox:1:")).ToList();
            var textboxOne = result.Elements.Where(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox:2:")).ToList();

            // Assert
            Assert.Equal(2, result.Elements.Count(_ => _.Type.Equals(EElementType.Textbox)));
            Assert.Single(textboxZero);
            Assert.Single(textboxOne);
            Assert.True(textboxZero[0].Properties.IsDynamicallyGeneratedElement);
            Assert.True(textboxOne[0].Properties.IsDynamicallyGeneratedElement);
        }
    }
}
