using Microsoft.AspNetCore.Hosting;
using form_builder.Enum;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class TemplatedEmailActionCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();

        public TemplatedEmailActionCheckTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                            .Returns("local");
        }

        [Theory]
        [InlineData("", "FAILURE - Templated Email Action, there is no 'EmailTemplateProvider'")]
        [InlineData("questionId", "FAILURE - Templated Email Action, there is no 'TemplateId' provided")]
        public void
        TemplatedEmailActionCheck_ShouldThrowException_WhenActionDoesNotContain_Provider_or_TemplatedId(string questionId, string message)
        {
            // Arrange
            var action = new ActionBuilder()
                .WithActionType(EActionType.TemplatedEmail)
                .WithTargetQuestionId(questionId)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithFormActions(action)
                .Build();

            var check = new TemplatedEmailActionCheck();

            // Act & Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Contains(message, result.Messages);
        }
    }
}
