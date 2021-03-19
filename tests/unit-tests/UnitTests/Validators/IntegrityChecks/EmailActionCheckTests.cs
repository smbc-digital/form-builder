using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class EmailActionCheckTests
    {
        [Theory]
        [InlineData("", "subject", "from", "to", "FAILURE - Email Actions Check, Content doesn't have a value")]
        [InlineData("content", "", "from", "to", "FAILURE - Email Actions Check, Subject doesn't have a value")]
        [InlineData("content", "subject", "", "to", "FAILURE - Email Actions Check, From doesn't have a value")]
        [InlineData("content", "subject", "from", "", "FAILURE - Email Actions Check, To doesn't have a value")]
        public void CheckEmailAction_IsNotValid_WhenActionDoesNotContain_Content_or_Subject_or_To_or_From(string content, string subject, string from, string to, string message)
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

            var check = new EmailActionsCheck();

            // Act & Assert
            var result = check.Validate(schema);

            Assert.False(result.IsValid);
            Assert.Contains(message, result.Messages);
        }
    }
}