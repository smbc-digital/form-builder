using form_builder.Enum;
using form_builder.Models;
using form_builder_tests.Builders;
using System;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Models
{
    public class PageTests
    {

        [Fact]
        public void GetSubmitFormEndpoint_ShouldReturnPageSlug_WhenOnlyOneBehaviourExistsAndNoSubmitSlugs()
        {
            var SubmitSlug = "page-one";

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug(SubmitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var result = page.GetSubmitFormEndpoint(new FormAnswers(), "");

            Assert.Equal(SubmitSlug, result.URL);
        }

        [Fact]
        public void GetSubmitFormEndpoint_ShouldReturnNull_WhenNoFormSubmitAction()
        {
            var PageSlug = "page-one";

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug(PageSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            Assert.Throws<NullReferenceException>(() => page.GetSubmitFormEndpoint(new FormAnswers(), null));
        }

        [Fact]
        public void GetSubmitFormEndpoint_ShouldReturnCorrectSubtmitActionUrl_WhenMultipleSubmits()
        {
            var PageSlug = "page-two";

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("page-one")
                .WithSubmitSlug(new SubmitSlug{ Environment = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug(PageSlug)
                .WithCondition(new Condition { EqualTo = "test", QuestionId = "test" })
                .WithSubmitSlug(new SubmitSlug{ Environment = "test", URL = PageSlug })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var result = page.GetSubmitFormEndpoint(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers> { new Answers { QuestionId = "test", Response = "test" } } } } }, "test");

            Assert.Equal(PageSlug, result.URL);
        }

        [Fact(Skip="WIP, test mmight not be valid as its GoToPage within Submit, will verify")]
        public void GetNextPage_ShouldGoToOther_WhenCheckboxContainsOther()
        {
            var PageSlug = "page-other";

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug(PageSlug)
                .WithCondition(new Condition { CheckboxContains = "other", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var result = page.GetSubmitFormEndpoint(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers> { new Answers { QuestionId = "test", Response = "test,other" } } } } }, null);

            Assert.Equal(PageSlug, result.URL);
        }
    }
}
