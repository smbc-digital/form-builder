using System.Collections.Generic;
using System.Linq;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class EmptyBehaviourSlugsCheckTests
    {
        [Fact]
        public void EmptyBehaviourSlugsCheck_IsNotValid_WhenSubmitSlugAndPageSlugAreEmpty()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            var check = new EmptyBehaviourSlugsCheck();

            // Act & Assert
            var result = check.Validate(schema);

            Assert.False(result.IsValid);
            Assert.Contains("FAILURE - Empty Behaviour Slugs Check, Incorrectly configured behaviour slug was discovered in 'test-name' form", result.Messages);
        }

        [Fact]
        public void EmptyBehaviourSlugsCheck_IsValid_WhenSubmitSlugIsNotEmpty()
        {
            // Arrange
            var submitSlug = new SubmitSlug
            {
                Environment = "local",
                URL = "test-url"
            };

            var behaviour = new BehaviourBuilder()
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            var check = new EmptyBehaviourSlugsCheck();

            // Act & Assert
            var result = check.Validate(schema);

            // Assert
            Assert.Single(schema.Pages[0].Behaviours[0].SubmitSlugs);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void CheckForEmptyBehaviourSlugs_ShouldNotThrowAnException_WhenPageSlugIsNotEmpty()
        {
            // Arrange
            var pages = new List<Page>();
            var behaviour = new BehaviourBuilder()
                .WithPageSlug("page-slug")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            var check = new EmptyBehaviourSlugsCheck();

            // Act & Assert
            var result = check.Validate(schema);

            // Assert
            Assert.Equal("page-slug", schema.Pages[0].Behaviours[0].PageSlug);
            Assert.True(result.IsValid);
        }
    }
}