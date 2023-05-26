using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Factories.Transform.UserSchema;
using form_builder.Models;
using form_builder.Models.Elements;
using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class AddAnotherPageTransformFactoryTests
    {
        private readonly AddAnotherPageTransformFactory _transformFactory;
        private readonly Page _page;

        private readonly FormAnswers _savedAnswers = new FormAnswers
        {
            FormData = new Dictionary<string, object>
            {
                { $"{AddAnotherConstants.IncrementKeyPrefix}person", 2 }
            }
        };

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
                .WithAppendText("person")
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { textboxElement };

            _page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(addAnotherElement)
                .Build();



            _transformFactory = new AddAnotherPageTransformFactory();
        }

        [Fact]
        public async Task Transform_ShouldReturnCorrectNumberOfElementsOnPage()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);

            // Assert
            Assert.Equal(12, result.Elements.Count);
        }

        [Fact]
        public async Task Transform_ShouldRetainOriginalAddAnotherElement()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);

            // Assert
            Assert.Single(result.Elements.Where(_ => _.Type.Equals(EElementType.AddAnother)));
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
        public async Task Transform_ShouldReturnCorrectNumberOf_LegendElements()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);

            // Assert
            Assert.Equal(2, result.Elements.Count(_ => _.Type.Equals(EElementType.Legend)));
        }

        [Theory]
        [InlineData(2, 4, 2)]
        [InlineData(3, 6, 3)]
        [InlineData(4, 8, 4)]
        [InlineData(5, 10, 5)]
        [InlineData(6, 12, 6)]
        [InlineData(7, 14, 7)]
        public async Task Transform_ShouldReturnCorrectNumberOf_FieldsetElements_WhenMinimumFieldsetsUsed(int minimumFieldsets, int expectedFieldsets, int expectedFieldsetOpenClose)
        {
            // Arrange
            var savedAnswers = new FormAnswers();

            var textboxElement = new ElementBuilder()
                .WithQuestionId("textbox")
                .WithType(EElementType.Textbox)
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithLabel("Person")
                .WithQuestionId("person")
                .WithType(EElementType.AddAnother)
                .WithNestedElement(textboxElement)
                .WithMinimumFieldsets(minimumFieldsets)
                .WithMaximumFieldsets(8)
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { textboxElement };

            var page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(addAnotherElement)
                .Build();

            // Act
            var result = await _transformFactory.Transform(page, savedAnswers);
            var openingFieldsets = result.Elements.Count(_ => _.Type.Equals(EElementType.Fieldset) && _.Properties.OpeningTag);
            var closingFieldsets = result.Elements.Count(_ => _.Type.Equals(EElementType.Fieldset) && !_.Properties.OpeningTag);

            // Assert
            Assert.Equal(expectedFieldsets, result.Elements.Count(_ => _.Type.Equals(EElementType.Fieldset)));
            Assert.Equal(expectedFieldsetOpenClose, openingFieldsets);
            Assert.Equal(expectedFieldsetOpenClose, closingFieldsets);
        }

        [Theory]
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        [InlineData(4, 4)]
        [InlineData(5, 5)]
        [InlineData(6, 6)]
        [InlineData(7, 7)]
        public async Task Transform_ShouldReturnCorrectLegendElements_WhenMinimumFieldsetsUsed(int minimumFieldsets, int expectedLegends)
        {
            // Arrange
            var savedAnswers = new FormAnswers();

            var textboxElement = new ElementBuilder()
                .WithQuestionId("textbox")
                .WithType(EElementType.Textbox)
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithLabel("Next person")
                .WithFirstLabel("First person")
                .WithQuestionId("person")
                .WithType(EElementType.AddAnother)
                .WithNestedElement(textboxElement)
                .WithMinimumFieldsets(minimumFieldsets)
                .WithMaximumFieldsets(8)
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { textboxElement };

            var page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(addAnotherElement)
                .Build();
            // Act
            var result = await _transformFactory.Transform(page, savedAnswers);

            // Assert
            Assert.Equal(expectedLegends, result.Elements.Count(_ => _.Type.Equals(EElementType.Legend)));
            Assert.Equal(addAnotherElement.Properties.FirstLabel, result.Elements.First(_ => _.Type.Equals(EElementType.Legend)).Properties.Label);
            Assert.Equal(addAnotherElement.Properties.Label, result.Elements.Last(_ => _.Type.Equals(EElementType.Legend)).Properties.Label);
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
        public async Task Transform_ShouldNotReturnRemoveButtonElement_WhenAtMinimumFieldsets()
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
                .WithMinimumFieldsets(2)
                .WithMaximumFieldsets(3)
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { textboxElement };

            var page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(addAnotherElement)
                .Build();

            var result = await _transformFactory.Transform(page, _savedAnswers);
            var removeButtons = result.Elements.Where(_ => _.Type.Equals(EElementType.Button) && _.Properties.ButtonName.Contains("remove-")).ToList();

            // Assert
            Assert.Equal(1, result.Elements.Count(_ => _.Type.Equals(EElementType.Button)));
            Assert.Empty(removeButtons);
        }

        [Fact]
        public async Task Transform_ShouldReturnCorrect_TextboxElements()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);
            var textboxZero = result.Elements.Where(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox_1_")).ToList();
            var textboxOne = result.Elements.Where(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox_2_")).ToList();

            // Assert
            Assert.Equal(2, result.Elements.Count(_ => _.Type.Equals(EElementType.Textbox)));
            Assert.Single(textboxZero);
            Assert.Single(textboxOne);
            Assert.True(textboxZero[0].Properties.IsDynamicallyGeneratedElement);
            Assert.True(textboxOne[0].Properties.IsDynamicallyGeneratedElement);
        }

        [Fact]
        public async Task Transform_ShouldReturnCorrectOptions_ForElementWithConditionalElements()
        {
            // Arrange
            var options = new List<Option>
            {
                new Option
                {
                    ConditionalElementId = "optionOne"
                }
            };

            var radioElement = new ElementBuilder()
                .WithQuestionId("radio")
                .WithType(EElementType.Radio)
                .WithOptions(options)
                .Build();

            var textbox = new ElementBuilder()
                .WithQuestionId("optionOne")
                .WithType(EElementType.Textbox)
                .WithIsConditionalElement(true)
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithLabel("Person")
                .WithQuestionId("person")
                .WithType(EElementType.AddAnother)
                .WithNestedElement(radioElement)
                .WithNestedElement(textbox)
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { radioElement };

            var page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(addAnotherElement)
                .Build();

            // Act
            var result = await _transformFactory.Transform(page, _savedAnswers);
            var radioOne = result.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.Radio) && _.Properties.QuestionId.Equals("radio_1_"));

            // Assert
            Assert.Equal("optionOne_1_", radioOne.Properties.Options[0].ConditionalElementId);
        }

        [Fact]
        public async Task Transform_ShouldSetAutofocus()
        {
            // Arrange
            var textbox = new ElementBuilder()
                .WithQuestionId("textbox")
                .WithSetAutofocus(true)
                .WithType(EElementType.Textbox)
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithLabel("Person")
                .WithQuestionId("person")
                .WithType(EElementType.AddAnother)
                .WithNestedElement(textbox)
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { textbox };

            var page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(addAnotherElement)
                .Build();

            // Act
            var result = await _transformFactory.Transform(page, _savedAnswers);
            var firstTextbox = result.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox_1_") && _.Properties.SetAutofocus);
            var secondTextbox = result.Elements.Last(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox_2_") && _.Properties.SetAutofocus);

            // Assert
            Assert.False(firstTextbox.Properties.Autofocus);
            Assert.True(secondTextbox.Properties.Autofocus);
        }

        [Fact]
        public async Task Transform_ShouldSetAutofocus_ToFirstElement_WhenMinimumFieldsetAbove1_AndNoSavedAnswers()
        {
            // Arrange
            var savedAnswers = new FormAnswers();
            var textbox = new ElementBuilder()
                .WithQuestionId("textbox")
                .WithSetAutofocus(true)
                .WithType(EElementType.Textbox)
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithLabel("Person")
                .WithQuestionId("person")
                .WithType(EElementType.AddAnother)
                .WithMinimumFieldsets(2)
                .WithNestedElement(textbox)
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { textbox };

            var page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(addAnotherElement)
                .Build();

            // Act
            var result = await _transformFactory.Transform(page, savedAnswers);
            var firstTextbox = result.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox_1_") && _.Properties.SetAutofocus);
            var secondTextbox = result.Elements.Last(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox_2_") && _.Properties.SetAutofocus);

            // Assert
            Assert.True(firstTextbox.Properties.Autofocus);
            Assert.False(secondTextbox.Properties.Autofocus);
        }
    }
}
