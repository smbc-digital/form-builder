using form_builder.Constants;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks.Behaviours;
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
            var behaviours = new List<Behaviour>
            {
            new BehaviourBuilder()
                .Build()
            };

            EmptyBehaviourSlugsCheck check = new();

            // Act & Assert
            var result = check.Validate(behaviours);

            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
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

            var behaviours = new List<Behaviour>
            {
            new BehaviourBuilder()
                .WithSubmitSlug(submitSlug)
                .Build()
            };

            EmptyBehaviourSlugsCheck check = new();

            // Act & Assert
            var result = check.Validate(behaviours);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }

        [Fact]
        public void CheckForEmptyBehaviourSlugs_ShouldNotThrowAnException_WhenPageSlugIsNotEmpty()
        {
            // Arrange
            var behaviours = new List<Behaviour>
            {
            new BehaviourBuilder()
                .WithPageSlug("page-slug")
                .Build()
            };

            EmptyBehaviourSlugsCheck check = new();

            // Act & Assert
            var result = check.Validate(behaviours);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}