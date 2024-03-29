﻿using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class OptionalIfCheckTests
    {
        [Fact]
        public void CheckForOptionalIf_ShouldThrowException_IfOptionalIfDoesNot_MatchQuestionId()
        {
            // Arrange
            var textbox = new ElementBuilder()
                 .WithQuestionId("textbox")
                 .WithType(EElementType.Textbox)
                 .WithOptional(false)
                 .Build();

            var textboxWithOptionalIf = new ElementBuilder()
                 .WithQuestionId("textboxWithOptionalIf")
                 .WithType(EElementType.Textbox)
                 .WithOptionalIf("test2", "test", ECondition.EqualTo)
                 .WithOptional(false)
                 .Build();

            var page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(textboxWithOptionalIf)
                .WithElement(textbox)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            OptionalIfCheck check = new();

            //Act & Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Theory]
        [InlineData("", "test", ECondition.EqualTo)]
        [InlineData("textbox", "", ECondition.EqualTo)]
        [InlineData("textbox", "test", ECondition.Undefined)]
        public void CheckForOptionalIf_ShouldThrowException_IfOptionalIf_DoesNotContain_MandatoryProperties(string questionId, string comparisonValue, ECondition conditionType)
        {
            // Arrange
            var textbox = new ElementBuilder()
                 .WithQuestionId("textbox")
                 .WithType(EElementType.Textbox)
                 .WithOptional(false)
                 .Build();

            var textboxWithOptionalIf = new ElementBuilder()
                 .WithQuestionId("textboxWithOptionalIf")
                 .WithType(EElementType.Textbox)
                 .WithOptionalIf(questionId, comparisonValue, conditionType)
                 .WithOptional(false)
                 .Build();

            var page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(textboxWithOptionalIf)
                .WithElement(textbox)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            OptionalIfCheck check = new();

            // Act & Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}
