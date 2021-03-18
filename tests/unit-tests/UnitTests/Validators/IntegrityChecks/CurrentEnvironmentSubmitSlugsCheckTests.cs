using System.Linq;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class CurrentEnvironmentSubmitSlugsCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();
        
        public CurrentEnvironmentSubmitSlugsCheckTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                    .Returns("local");
        }

        [Fact]
        public void CurrentEnvironmentSubmitSlugsCheck_IsNotValid_WhenPageSlugIsNotPresentFor()
        {
            // Arrange
            var submitSlug = new SubmitSlug
            {
                Environment = "mysteryEnvironment",
                URL = "test-url"
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            var check = new CurrentEnvironmentSubmitSlugsCheck(_mockHostingEnv.Object);

            // Act & Assert
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Messages, _ => _.Equals($"FAILURE - No SubmitSlug found in form 'test-name' for environment 'local'."));
        }
    }
}