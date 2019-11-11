using form_builder.Enum;
using form_builder.Models;
using form_builder_tests.Builders;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Models
{
    public class PageTests
    {

        [Fact]
        public void GetSubmitFormEndpoint_ShouldReturnFirstBehviourSubmitUrl_WhenOnlyOneBehaviourExists()
        {
            var pageUrl = "page-one";

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageUrl(pageUrl)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var result = page.GetSubmitFormEndpoint(new FormAnswers());

            Assert.Equal(pageUrl, result);
        }

        [Fact]
        public void GetSubmitFormEndpoint_ShouldReturnNull_WhenNoFormSubmitAction()
        {
            var pageUrl = "page-one";

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageUrl(pageUrl)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var result = page.GetSubmitFormEndpoint(new FormAnswers());

            Assert.Null(result);
        }

        [Fact]
        public void GetSubmitFormEndpoint_ShouldReturnCorrectSubtmitActionUrl_WhenMultipleSubmits()
        {
            var pageUrl = "page-two";

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageUrl("page-one")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageUrl(pageUrl)
                .WithCondition(new Condition { EqualTo = "test", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var result = page.GetSubmitFormEndpoint(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers> { new PageAnswers { PageUrl = "page-one", Answers = new List<Answers> { new Answers { QuestionId = "test", Response = "test" } } } } });

            Assert.Equal(pageUrl, result);
        }
    }
}
