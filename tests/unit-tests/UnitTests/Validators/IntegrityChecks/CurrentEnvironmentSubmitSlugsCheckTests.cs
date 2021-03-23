using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks.Behaviours;
using form_builder_tests.Builders;

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

            var behaviours = new List<Behaviour>
            {
                new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .Build()
            };

            var check = new CurrentEnvironmentSubmitSlugsCheck(_mockHostingEnv.Object);

            // Act & Assert
            var result = check.Validate(behaviours);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}