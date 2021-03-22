using System.Linq;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class IncomingFormDataValuesCheckTests
    {
        [Fact]
        public void IncomingFormDataValuesCheck_IsNotValid_WhenQuestionId_OR_Name_Empty()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValueWithNoQuestionId = new IncomingValuesBuilder()
                .WithQuestionId("")
                .WithName("testName")
                .WithHttpActionType(EHttpActionType.Post)
                .Build();

            var incomingValueWithNoName = new IncomingValuesBuilder()
                .WithQuestionId("testQuestionId")
                .WithName("")
                .WithHttpActionType(EHttpActionType.Post)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValueWithNoQuestionId)
                .WithIncomingValue(incomingValueWithNoName)
                .WithPageTitle("test-page")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-form")
                .Build();

            var check = new IncomingFormDataValuesCheck();

            // Act & Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Contains("FAILURE - Incoming Form DataValues Check, QuestionId or Name cannot be empty on page 'test-page' in form 'test-form'", result.Messages);
        }

        [Fact]
        public void IncomingFormDataValuesCheck_IsNotValid_WhenActionType_IsUnknown()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValueWithNoType = new IncomingValuesBuilder()
                .WithQuestionId("testQuestionId")
                .WithName("test-value")
                .WithHttpActionType(EHttpActionType.Unknown)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValueWithNoType)
                .WithPageTitle("test-page")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-form")
                .Build();

            var check = new IncomingFormDataValuesCheck();

            // Act & Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Contains("FAILURE - Incoming Form DataValues Check, EHttpActionType cannot be unknown, set to Get or Post for incoming value 'test-value' on page 'test-page' in form 'test-form'", result.Messages);
        }
    }
}
