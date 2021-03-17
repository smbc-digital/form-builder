using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class ConditionalElementsAreValidCheckTests
    {
        
        [Fact]
        public void ConditionalElementsAreValidCheck_IsValid_WhenConditionalElementIsFoundInJson()
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .WithLabel("First name")
                .WithOptions(new List<Option> { option1 })
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithLabel("First name")
                .WithConditionalElement(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element1)
                .WithElement(element2)
                .Build();
            
            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            var check = new ConditionalElementsAreValidCheck();

            // Act & Assert
            var result = check.Validate(schema);

            // Assert            
            Assert.True(result.IsValid);
        }

        [Fact]
        public void CheckConditionalElementsAreValid_ShouldNotThrowApplicationException_WhenConditionalElementIdIsBlankInJson()
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = "", Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .WithLabel("First name")
                .WithOptions(new List<Option> { option1 })
                .Build();

            var page = new PageBuilder()
                .WithElement(element1)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            var check = new ConditionalElementsAreValidCheck();

            // Act & Assert
            var result = check.Validate(schema);

            // Assert            
            Assert.True(result.IsValid);
        }

        [Fact]
        public void CheckConditionalElementsAreValid_ShouldThrowApplicationException_WhenConditionalElementNotFoundInJson()
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .WithLabel("First name")
                .WithOptions(new List<Option> { option1 })
                .Build();

            var page = new PageBuilder()
                .WithElement(element1)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            var check = new ConditionalElementsAreValidCheck();

            // Act & Assert
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CheckConditionalElementsAreValid_ShouldThrowApplicationException_WhenTooManyConditionalElementsFoundInJson()
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .WithLabel("First name")
                .WithOptions(new List<Option> { option1 })
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithLabel("First name")
                .WithConditionalElement(true)
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion2")
                .WithLabel("First name")
                .WithConditionalElement(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element1)
                .WithElement(element2)
                .WithElement(element3)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            var check = new ConditionalElementsAreValidCheck();

            // Act & Assert
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CheckConditionalElementsAreValid_ShouldThrowApplicationException_WhenConditionalElementIsPlacedOnAnotherPageInJson()
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .WithLabel("First name")
                .WithOptions(new List<Option> { option1 })
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithLabel("First name")
                .WithConditionalElement(true)
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element2)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page1)
                .WithPage(page2)
                .WithName("test-name")
                .Build();

            var check = new ConditionalElementsAreValidCheck();

            // Act & Assert
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}