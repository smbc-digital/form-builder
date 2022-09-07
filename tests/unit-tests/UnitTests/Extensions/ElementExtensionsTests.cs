using form_builder.Builders;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class ElementExtensionsTests
    {
        [Fact]
        public void RemoveUnusedConditionalElements_ViewModelShouldContainLessElementsWhenElementsContainMultipleConditionals_ForRadio()
        {
            // Arrange
            List<IElement> elements = new List<IElement>();
            var conditionalElement1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithValue("Conditional Answer 1")
                .WithConditionalElement(true)
                .Build();
            var conditionalElement2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion2")
                .WithValue("Conditional Answer 2")
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
            var viewModel = new Dictionary<string, dynamic>
            {
                { "testQuestion", "Value1" },
                { "conditionalQuestion1", "Conditional Answer 1" },
                { "conditionalQuestion2", "Conditional Answer 2" }
            };

            // Act
            List<IElement> listOfElements = elements.RemoveUnusedConditionalElements(viewModel);

            // Assert
            Assert.Equal(2, listOfElements.Count);
            Assert.Equal(2, viewModel.Count);
        }

        [Fact]
        public void RemoveUnusedConditionalElements_ShouldKeepOriginalViewModelWhenNoConditionalElementsInList()
        {
            // Arrange
            List<IElement> elements = new List<IElement>();
            var viewModel = new Dictionary<string, dynamic> { { "testQuestion", "Value" } };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("testQuestion")
                .Build();
            elements.Add(element1);

            // Act
            List<IElement> listOfElements = elements.RemoveUnusedConditionalElements(viewModel);

            // Assert
            Assert.Single(listOfElements);
            Assert.Single(viewModel);
        }

        [Fact]
        public void RemoveUnusedConditionalElements_ViewModelShouldContain_SameAmountOf_ElementsWhen_Checkbox_Has_Selected_BothOptions()
        {
            // Arrange
            List<IElement> elements = new List<IElement>();
            var conditionalElement1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithValue("Conditional Answer 1")
                .WithConditionalElement(true)
                .Build();
            var conditionalElement2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion2")
                .WithValue("Conditional Answer 2")
                .WithConditionalElement(true)
                .Build();
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var option2 = new Option { ConditionalElementId = "conditionalQuestion2", Value = "Value2" };
            var listOfOptions = new List<Option> { option1, option2 };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId("testQuestion")
                .WithOptions(listOfOptions)
                .Build();
            elements.Add(conditionalElement1);
            elements.Add(conditionalElement2);
            elements.Add(element1);
            var viewModel = new Dictionary<string, dynamic>
            {
                { "testQuestion", "Value1,Value2" },
                { "conditionalQuestion1", "Conditional Answer 1" },
                { "conditionalQuestion2", "Conditional Answer 2" }
            };

            // Act
            List<IElement> listOfElements = elements.RemoveUnusedConditionalElements(viewModel);

            // Assert
            Assert.Equal(3, listOfElements.Count);
            Assert.Equal(3, viewModel.Count);
        }


        [Fact]
        public void RemoveUnusedConditionalElements_ViewModelShould_HaveOneLessItem_WhenOnlySingleCheckboxValue_IsTicked()
        {
            // Arrange
            List<IElement> elements = new List<IElement>();
            var conditionalElement1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithValue("Conditional Answer 1")
                .WithConditionalElement(true)
                .Build();
            var conditionalElement2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion2")
                .WithValue("Conditional Answer 2")
                .WithConditionalElement(true)
                .Build();
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var option2 = new Option { ConditionalElementId = "conditionalQuestion2", Value = "Value2" };
            var listOfOptions = new List<Option> { option1, option2 };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId("testQuestion")
                .WithOptions(listOfOptions)
                .Build();
            elements.Add(conditionalElement1);
            elements.Add(conditionalElement2);
            elements.Add(element1);
            var viewModel = new Dictionary<string, dynamic>
            {
                { "testQuestion", "Value1" },
                { "conditionalQuestion1", "Conditional Answer 1" },
                { "conditionalQuestion2", "Conditional Answer 2" }
            };

            // Act
            List<IElement> listOfElements = elements.RemoveUnusedConditionalElements(viewModel);

            // Assert
            Assert.Equal(2, listOfElements.Count);
            Assert.Equal(2, viewModel.Count);
        }

        [Fact]
        public void RemoveUnusedConditionalElements_ViewModelShould_Contain_SingleItem_WhenNoOptions_AreTicked_ForCheckbox()
        {
            // Arrange
            List<IElement> elements = new List<IElement>();
            var conditionalElement1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithValue("Conditional Answer 1")
                .WithConditionalElement(true)
                .Build();
            var conditionalElement2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion2")
                .WithValue("Conditional Answer 2")
                .WithConditionalElement(true)
                .Build();
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var option2 = new Option { ConditionalElementId = "conditionalQuestion2", Value = "Value2" };
            var listOfOptions = new List<Option> { option1, option2 };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId("testQuestion")
                .WithOptions(listOfOptions)
                .Build();
            elements.Add(conditionalElement1);
            elements.Add(conditionalElement2);
            elements.Add(element1);
            var viewModel = new Dictionary<string, dynamic>
            {
                { "testQuestion", "" },
                { "conditionalQuestion1", "Conditional Answer 1" },
                { "conditionalQuestion2", "Conditional Answer 2" }
            };

            // Act
            List<IElement> listOfElements = elements.RemoveUnusedConditionalElements(viewModel);

            // Assert
            Assert.Single(listOfElements);
            Assert.Single(viewModel);
        }

        [Fact]
        public void RemoveUnusedConditionalElements_ElementListShould_Contain_OnlyConditionalElementsThatHaveCorrespondingSelectedOptions()
        {
            // Arrange
            List<IElement> elements = new List<IElement>();
            var conditionalElement1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithValue("Conditional Answer 1")
                .WithConditionalElement(true)
                .Build();
            var conditionalElement2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion2")
                .WithValue("Conditional Answer 2")
                .WithConditionalElement(true)
                .Build();
            var conditionalElement3 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion3")
                .WithConditionalElement(true)
                .Build();
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var option2 = new Option { ConditionalElementId = "conditionalQuestion2", Value = "Value2" };
            var listOfOptions = new List<Option> { option1, option2 };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId("testQuestion")
                .WithOptions(listOfOptions)
                .Build();
            elements.Add(conditionalElement1);
            elements.Add(conditionalElement2);
            elements.Add(conditionalElement3);
            elements.Add(element1);
            var viewModel = new Dictionary<string, dynamic>
            {
                { "testQuestion", "Value1, Value2" },
                { "conditionalQuestion1", "Conditional Answer 1" },
                { "conditionalQuestion2", "Conditional Answer 2" }
            };

            // Act
            List<IElement> listOfElements = elements.RemoveUnusedConditionalElements(viewModel);

            // Assert
            Assert.Equal(3, listOfElements.Count);
            Assert.Equal(2, listOfElements.Count(_ => _.Properties.isConditionalElement));
            Assert.Equal(3, viewModel.Count);
        }

        [Fact]
        public void RemoveUnusedConditionalElements_ElementListShould_Contain_OnlyConditionalElementsThatHaveCorrespondingSelectedOptions_MultipleElementsWithConditional()
        {
            // Arrange
            List<IElement> elements = new List<IElement>();
            var conditionalElement1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("question1Conditional")
                .WithValue("Conditional Answer 1")
                .WithConditionalElement(true)
                .Build();
            var conditionalElement2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("question2Conditional")
                .WithValue("Conditional Answer 2")
                .WithConditionalElement(true)
                .Build();
            var conditionalElement3 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("question2Conditional2")
                .WithConditionalElement(true)
                .Build();
            var q1option1 = new Option { Value = "Value1" };
            var q1option2 = new Option { ConditionalElementId = "question1Conditional", Value = "Value2" };
            var q2option1 = new Option { ConditionalElementId = "question2Conditional", Value = "Value1" };
            var q2option2 = new Option { ConditionalElementId = "question2Conditional2", Value = "Value2" };
            var q1ListOfOptions = new List<Option> { q1option1, q1option2 };
            var q2ListOfOptions = new List<Option> { q2option1, q2option2 };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("question1")
                .WithOptions(q1ListOfOptions)
                .Build();
            var element2 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("question2")
                .WithOptions(q2ListOfOptions)
                .Build();
            elements.Add(conditionalElement1);
            elements.Add(conditionalElement2);
            elements.Add(conditionalElement3);
            elements.Add(element1);
            elements.Add(element2);
            var viewModel = new Dictionary<string, dynamic>
            {
                { "question1", "Value2" },
                { "question2", "Value1" },
                { "question1Conditional", "Conditional Answer 1" },
                { "question2Conditional", "Conditional Answer 2" },
                { "question2Conditional2", "Conditional Answer 3" }
            };

            // Act
            List<IElement> listOfElements = elements.RemoveUnusedConditionalElements(viewModel);

            // Assert
            Assert.Equal(4, listOfElements.Count);
            Assert.Equal(2, listOfElements.Count(_ => _.Properties.isConditionalElement));
            Assert.Equal(4, viewModel.Count);
        }
    }
}
