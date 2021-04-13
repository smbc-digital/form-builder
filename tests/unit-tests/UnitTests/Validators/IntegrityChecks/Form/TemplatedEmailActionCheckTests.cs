using System.Linq;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class TemplatedEmailActionCheckTests
    {
        [Theory]
        [InlineData("", "templateId", "emailAddress", "FAILURE - Templated Email Action, there is no 'EmailTemplateProvider'")]
        [InlineData("provider", "", "emailAddress", "FAILURE - Templated Email Action, there is no 'TemplateId' provided")]
        [InlineData("provider", "templateId", "", "FAILURE - Templated Email Action, there is no 'To' provided")]
        public void TemplatedEmailActionCheck_ShouldAddCorrectErrorMessage_WhenActionIsMissingProperties(
            string provider, string templateId, string to, string expectedErrorMessage)
        {
            // Arrange
            var action = new ActionBuilder()
                .WithActionType(EActionType.TemplatedEmail)
                .WithProvider(provider)
                .WithTemplateId(templateId)
                .WithTo(to)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithFormActions(action)
                .Build();

            var check = new TemplatedEmailActionCheck();

            // Act & Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Equal(expectedErrorMessage, result.Messages.First());
        }
    }
}
