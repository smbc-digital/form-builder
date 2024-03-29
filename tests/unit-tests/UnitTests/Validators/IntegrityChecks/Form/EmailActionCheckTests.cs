using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class EmailActionCheckTests
    {
        [Theory]
        [InlineData("", "subject", "from", "to")]
        [InlineData("content", "", "from", "to")]
        [InlineData("content", "subject", "", "to")]
        [InlineData("content", "subject", "from", "")]
        public void CheckEmailAction_IsNotValid_WhenActionDoesNotContain_Content_or_Subject_or_To_or_From(string content, string subject, string from, string to)
        {
            // Arrange
            var action = new ActionBuilder()
                .WithActionType(EActionType.UserEmail)
                .WithContent(content)
                .WithSubject(subject)
                .WithFrom(from)
                .WithTo(to)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithFormActions(action)
                .Build();

            EmailActionsCheck check = new();

            // Act & Assert
            var result = check.Validate(schema);

            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}