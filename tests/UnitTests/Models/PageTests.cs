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
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug(PageSlug)
                .WithCondition(new Condition { EqualTo = "test", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var result = page.GetSubmitFormEndpoint(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers> { new Answers { QuestionId = "test", Response = "test" } } } } }, null);

            Assert.Equal(PageSlug, result.URL);
        }

        [Fact]
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

       
[Fact]
        public void GetNextPage_ShouldGoToOther_WhenDateLessThan42Days()
        {
            var PageSlugLessThan = "less-than";
            var PageSlugGreaterThan = "more-than";



            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug(PageSlugLessThan)
                .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsBefore = 42, Unit = EDateUnit.Day, QuestionId = "testDate" })
                .Build();


            var behaviour3 = new BehaviourBuilder()
               .WithBehaviourType(EBehaviourType.GoToPage)
               .WithPageSlug(PageSlugGreaterThan)
               .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsAfter = 43, Unit = EDateUnit.Day, QuestionId = "testDate" })
               .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var result = page.GetSubmitFormEndpoint(new FormAnswers
            { Path = "page-one", 
                Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", 
                    Answers = new List<Answers> { 
                new Answers { QuestionId = "testDate-year", Response = DateTime.Today.Year.ToString() },
                new Answers { QuestionId = "testDate-month", Response = DateTime.Today.Month.ToString() },
                new Answers { QuestionId = "testDate-day", Response = DateTime.Today.Day.ToString() }


            } } } }, null);

            Assert.Equal(PageSlugLessThan, result.URL);

        }

        [Fact]
        public void GetNextPage_ShouldGoToOther_WhenDateGreaterThan42Days()
        {
            var PageSlugLessThan = "less-than";
            var PageSlugGreaterThan = "more-than";



            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug(PageSlugLessThan)
                .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsBefore = 42, Unit = EDateUnit.Day, QuestionId = "testDate" })
                .Build();


            var behaviour3 = new BehaviourBuilder()
               .WithBehaviourType(EBehaviourType.GoToPage)
               .WithPageSlug(PageSlugGreaterThan)
               .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsAfter = 43, Unit = EDateUnit.Day, QuestionId = "testDate" })
               .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var futureDate = DateTime.Today.AddDays(50);

            var result = page.GetSubmitFormEndpoint(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one",
                    Answers = new List<Answers> {
                new Answers { QuestionId = "testDate-year", Response = futureDate.Year.ToString() },
                new Answers { QuestionId = "testDate-month", Response = futureDate.Month.ToString() },
                new Answers { QuestionId = "testDate-day", Response = futureDate.Day.ToString() }


            } } }
            }, null);

            Assert.Equal(PageSlugGreaterThan, result.URL);

        }
        [Fact]
        public void GetNextPage_ShouldGoToOther_WhenDateLessThan42DaysWithCalendarControl()
        {
            var PageSlugLessThan = "less-than";
            var PageSlugGreaterThan = "more-than";

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug(PageSlugLessThan)
                .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsBefore = 42, Unit = EDateUnit.Day, QuestionId = "testDate" })
                .Build();


            var behaviour3 = new BehaviourBuilder()
               .WithBehaviourType(EBehaviourType.GoToPage)
               .WithPageSlug(PageSlugGreaterThan)
               .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsAfter = 43, Unit = EDateUnit.Day, QuestionId = "testDate" })
               .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var result = page.GetSubmitFormEndpoint(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one",
                    Answers = new List<Answers> {
                new Answers { QuestionId = "testDate", Response = DateTime.Today.ToString() }
            } } }
            }, null);

            Assert.Equal(PageSlugLessThan, result.URL);

        }

        [Fact]
        public void GetNextPage_ShouldGoToOther_WhenDateGreaterThan42DaysWithCalendarControl()
        {
            var PageSlugLessThan = "less-than";
            var PageSlugGreaterThan = "more-than";



            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug(PageSlugLessThan)
                .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsBefore = 42, Unit = EDateUnit.Day, QuestionId = "testDate" })
                .Build();


            var behaviour3 = new BehaviourBuilder()
               .WithBehaviourType(EBehaviourType.GoToPage)
               .WithPageSlug(PageSlugGreaterThan)
               .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsAfter = 43, Unit = EDateUnit.Day, QuestionId = "testDate" })
               .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var futureDate = DateTime.Today.AddDays(50);

            var result = page.GetSubmitFormEndpoint(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one",
                    Answers = new List<Answers> {
                new Answers { QuestionId = "testDate", Response = futureDate.ToString("yyyy-MM-dd") },

            } } }
            }, null);

            Assert.Equal(PageSlugGreaterThan, result.URL);

        }
    }
}
