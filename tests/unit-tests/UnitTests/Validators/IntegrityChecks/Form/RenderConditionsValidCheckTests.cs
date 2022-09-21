using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class RenderConditionsValidCheckTests
    {
        [Fact]
        public void RenderConditionsValidCheck_IsNotValid_If_TwoOrMorePagesWithTheSameSlug_HaveNoRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .WithPage(page2)
                .Build();

            // Act
            var check = new RenderConditionsValidCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains($"FAILURE - Render Conditions Valid Check, More than one {page.PageSlug} page has no render conditions", result.Messages);
        }

        [Fact]
        public void RenderConditionsValidCheck_IsNotValid_If_TwoOrMorePagesWithTheSameSlug_HaveEmptyRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            page.RenderConditions = new List<Condition>();
            page2.RenderConditions = new List<Condition>();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .WithPage(page2)
                .Build();

            // Act
            var check = new RenderConditionsValidCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains($"FAILURE - Render Conditions Valid Check, More than one {page.PageSlug} page has no render conditions", result.Messages);
        }

        [Fact]
        public void RenderConditionsValidCheck_IsNotValid_If_TwoOrMorePagesWithTheSameSlug_HaveEmptyRenderConditions_And_TheLastPageHasRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var page3 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "yes"
                })
                .Build();

            page.RenderConditions = new List<Condition>();
            page2.RenderConditions = new List<Condition>();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .WithPage(page2)
                .WithPage(page3)
                .Build();

            // Act
            var check = new RenderConditionsValidCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains($"FAILURE - Render Conditions Valid Check, More than one {page.PageSlug} page has no render conditions", result.Messages);
        }

        [Fact]
        public void RenderConditionsValidCheck_IsValid_If_TwoPagesHaveTheSameSlug_And_TheFirstOneHasRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "yes"
                })
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            page2.RenderConditions = new List<Condition>();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .WithPage(page2)
                .Build();

            // Act
            var check = new RenderConditionsValidCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void RenderConditionsValidCheck_IsValid_If_TwoPagesWithTheSameSlugHaveRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "yes"
                })
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "no"
                })
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .WithPage(page2)
                .Build();

            // Act
            var check = new RenderConditionsValidCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void RenderConditionsValidCheck_IsValid_If_TwoOrMorePagesWithTheSameSlugHaveRenderConditions_And_TheLastPageHasNoRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "yes"
                })
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "no"
                })
                .Build();

            var page3 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            page3.RenderConditions = new List<Condition>();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .WithPage(page2)
                .WithPage(page3)
                .Build();

            // Act
            var check = new RenderConditionsValidCheck();
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
