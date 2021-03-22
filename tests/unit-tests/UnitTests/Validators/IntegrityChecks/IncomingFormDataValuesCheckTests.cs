using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
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

            IncomingFormDataValuesCheck check = new();

            // Act & Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.All<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
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

            IncomingFormDataValuesCheck check = new();

            // Act & Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}
