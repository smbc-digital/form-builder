using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class ConditionalElementsAreValidCheckTests
    {
        [Theory]
        [InlineData(EElementType.Radio, "conditionalOne", "conditionalOne")]
        [InlineData(EElementType.Checkbox, "conditionalOne", "conditionalOne")]
        public void ConditionalElementsAreValidCheck_IsValid(
            EElementType elementType,
            string conditionalElementId,
            string conditionalElementCorrespondingId)
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = conditionalElementId, Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(elementType)
                .WithOptions(new List<Option> { option1 })
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId(conditionalElementCorrespondingId)
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

            // Act & Assert
            ConditionalElementCheck check = new();
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }

        [Theory]
        [InlineData(EElementType.Radio, "conditionalOne", "conditionalTwo")]
        [InlineData(EElementType.Checkbox, "conditionalOne", "conditionalTwo")]
        public void ConditionalElementsAreValidCheck_IsNotValid_NoCorrespondingConditionalId(
            EElementType elementType,
            string conditionalElementId,
            string conditionalElementCorrespondingId)
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = conditionalElementId, Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(elementType)
                .WithOptions(new List<Option> { option1 })
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId(conditionalElementCorrespondingId)
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

            // Act
            ConditionalElementCheck check = new();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.All<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Theory]
        [InlineData(EElementType.Radio, "conditionalOne")]
        [InlineData(EElementType.Checkbox, "conditionalOne")]
        public void ConditionalElementsAreValidCheck_IsNotValid_NoConditionalElement(
            EElementType elementType,
            string conditionalElementId)
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = conditionalElementId, Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(elementType)
                .WithOptions(new List<Option> { option1 })
                .Build();

            var page = new PageBuilder()
                .WithElement(element1)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            // Act
            ConditionalElementCheck check = new();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.All<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Theory]
        [InlineData(EElementType.Radio, "conditionalOne", "conditionalTwo")]
        [InlineData(EElementType.Checkbox, "conditionalOne", "conditionalTwo")]
        public void ConditionalElementsAreValidCheck_IsNotValid_ConditionalElementOnAnotherPage(
            EElementType elementType,
            string conditionalElementId,
            string conditionalElementCorrespondingId)
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = conditionalElementId, Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(elementType)
                .WithOptions(new List<Option> { option1 })
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId(conditionalElementCorrespondingId)
                .WithConditionalElement(true)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element2)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page1)
                .WithPage(page2)
                .WithName("test-name")
                .Build();

            // Act
            ConditionalElementCheck check = new();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.All<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}