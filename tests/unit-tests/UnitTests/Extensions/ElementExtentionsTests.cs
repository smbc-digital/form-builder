using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class ElementExtentionsTests
    {

        public ElementExtentionsTests()
        {

        }

        [Fact]
        public void RemoveUnusedConditionalElements_ViewModelShouldContainLessElementsWhenElementsContainMultipleConditionals()
        {
            List<IElement> elements = new List<IElement>();
            var conditionalElement1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithValue("Conditonal Answer 1")
                .WithConditionalElement(true)
                .Build();
            var conditionalElement2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion2")
                .WithValue("Conditonal Answer 2")
                .WithConditionalElement(true)
                .Build();
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var option2 = new Option { ConditionalElementId = "conditionalQuestion2", Value = "Value2" };
            var listOfOptions = new List<Option> { option1, option2 };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("testQuestion")
                .WithOptions(listOfOptions)
                .Build();
            elements.Add(conditionalElement1);
            elements.Add(conditionalElement2);
            elements.Add(element1);
            var viewModel = new Dictionary<string, dynamic> { { "testQuestion", "Value1" }, { "conditionalQuestion1", "Conditonal Answer 1" }, { "conditionalQuestion2", "Conditonal Answer 2" } };

            List<IElement> listOfElements = elements.RemoveUnusedConditionalElements(viewModel);

            Assert.Equal(2, listOfElements.Count);
            Assert.Equal(2, viewModel.Count);
        }

        [Fact]
        public void RemoveUnusedConditionalElements_ShouldKeepOriginalViewModelWhenNoCoditionalElementsInList()
        {
            List<IElement> elements = new List<IElement>();
            var viewModel = new Dictionary<string, dynamic> { { "testQuestion", "Value" } };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("testQuestion")
                .Build();
            elements.Add(element1);
            List<IElement> listOfElements = elements.RemoveUnusedConditionalElements(viewModel);

            Assert.Single(listOfElements);
            Assert.Single(viewModel);
        }
    }
}
