using System.Collections.Generic;
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
    public class SubmitSlugsHaveAllPropertiesCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();

        public SubmitSlugsHaveAllPropertiesCheckTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("local");
        }
        [Fact]
        public void SubmitSlugsHaveAllPropertiesCheck_IsNotValid_WhenAuthTokenIsNullOrEmptyAndBehaviourTypeIsNotSubmitPowerAutomate()
        {
            // Arrange
            var pages = new List<Page>();
            var submitSlugs = new SubmitSlug
            {
                URL = "test",
                Environment="local"
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            // Act
            var check = new SubmitSlugsHaveAllPropertiesCheck(_mockHostingEnv.Object);
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Messages, message => message.Equals($"FAILURE - No auth token found for SubmitSlug in environmment 'local' in form 'test-name'"));
        }

        [Fact]
        public void SubmitSlugsHaveAllProperties_IsNotValue_WhenUrlIsNull()
        {
            // Arrange
            var pages = new List<Page>();
            var submitSlugs = new SubmitSlug
            {
                Environment="local",
                AuthToken = "this is auth token"
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            // Act
            var check = new SubmitSlugsHaveAllPropertiesCheck(_mockHostingEnv.Object);
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Messages, message => message.Equals($"FAILURE - No URL found for SubmitSlug in environmment 'local' in form 'test-name'"));
        }

        [Fact]
        public void SubmitSlugsHaveAllProperties_IsNotValid_WhenUrlIsEmpty()
        {
            // Arrange
            var pages = new List<Page>();
            var submitSlugs = new SubmitSlug
            {
                AuthToken = "this is auth token",
                URL = "",
                Environment="local",
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            // Act
            var check = new SubmitSlugsHaveAllPropertiesCheck(_mockHostingEnv.Object);
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Messages, message => message.Equals($"FAILURE - No URL found for SubmitSlug in environmment 'local' in form 'test-name'"));
        }

        [Fact]
        public void CheckSubmitSlugsHaveAllProperties_IsValid_WhenAuthTokenAndUrlAreNotNullOrEmpty()
        {
            // Arrange
            var pages = new List<Page>();
            var submitSlugs = new SubmitSlug
            {
                AuthToken = "this is auth token",
                URL = "test"
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            // Act
            var check = new SubmitSlugsHaveAllPropertiesCheck(_mockHostingEnv.Object);
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}