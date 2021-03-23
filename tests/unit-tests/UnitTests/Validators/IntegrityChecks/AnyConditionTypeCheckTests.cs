using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class AnyConditionTypeCheckTests
    {
        [Fact]
        public void CheckForAnyConditionType_ShouldThrowException_IfComparisonValueIsNullOrEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            var condition = new ConditionBuilder()
                .WithConditionType(ECondition.Any)
                .WithQuestionId("test")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithCondition(condition)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            AnyConditionTypeCheck check = new();

            // Act & Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void CheckForAnyConditionType_ShouldAllowSchema_IfComparisonValueIsSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            var condition = new ConditionBuilder()
                .WithConditionType(ECondition.Any)
                .WithComparisonValue("compValue")
                .WithQuestionId("test")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithCondition(condition)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            AnyConditionTypeCheck check = new();

            // Act & Assert
            var result = check.Validate(schema);
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}