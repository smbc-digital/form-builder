using Microsoft.AspNetCore.Hosting;
using form_builder.Enum;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class RetrieveExternalActionsCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();

        public RetrieveExternalActionsCheckTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                            .Returns("local");
        }

        [Theory]
        [InlineData("", "questionId", "local",
            "FAILURE - Retrieve External Data Action, action type does not contain a url")]
        [InlineData("www.url.com", "", "local",
            "FAILURE - Retrieve External Data Action, action type does not contain a TargetQuestionId")]
        [InlineData("www.url.com", "questionId", "test",
            "FAILURE - Retrieve External Data Action, there is no PageActionSlug for environment 'local'")]
        public void
        CheckRetrieveExternalDataAction_ShouldThrowException_WhenActionDoesNotContain_URL_or_TargetQuestionId(string url, string questionId, string env, string message)
        {
            // Arrange
            var action = new ActionBuilder()
                .WithActionType(EActionType.RetrieveExternalData)
                .WithPageActionSlug(new PageActionSlug
                {
                    URL = url,
                    Environment = env
                })
                .WithTargetQuestionId(questionId)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithFormActions(action)
                .Build();

            var check = new RetrieveExternalActionsCheck(_mockHostingEnv.Object);

            // Act & Assert
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Contains(message, result.Messages);
        }
    }
}