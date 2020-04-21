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
                .WithSubmitSlug(new SubmitSlug { Environment = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug(PageSlug)
                .WithCondition(new Condition { EqualTo = "test", QuestionId = "test" })
                .WithSubmitSlug(new SubmitSlug { Environment = "test", URL = PageSlug })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var result = page.GetSubmitFormEndpoint(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers> { new Answers { QuestionId = "test", Response = "test" } } } } }, "test");

            Assert.Equal(PageSlug, result.URL);
        }

        [Fact(Skip = "WIP, test mmight not be valid as its GoToPage within Submit, will verify")]
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

        #region GetNextPage Tests

        [Theory]
        [InlineData(EBehaviourType.GoToPage)]
        [InlineData(EBehaviourType.GoToExternalPage)]
        [InlineData(EBehaviourType.SubmitAndPay)]
        [InlineData(EBehaviourType.SubmitForm)]
        public void GetNextPage_ShouldReturn_DefaultBehaviour(EBehaviourType type)
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(type)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var result = page.GetNextPage(new Dictionary<string, dynamic>());

            Assert.Equal(type, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_DefaultBehaviour_WhenEqualTo_Condition_False()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { EqualTo = "value", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "wrongvalue");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.GoToExternalPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenEqualTo_Condition_True()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { EqualTo = "value", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "value");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }


        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMultipleEqualTo_Condition_True()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithCondition(new Condition { EqualTo = "pear", QuestionId = "test" })
                .Build();

            var behaviour3 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithCondition(new Condition { EqualTo = "berry", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "pear");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.SubmitAndPay, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMultipleEqualToConditions_Condition_True()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithCondition(new Condition { EqualTo = "pear", QuestionId = "test" })
                .Build();

            var behaviour3 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithCondition(new Condition { EqualTo = "berry", QuestionId = "test" })
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test2" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "pear");
            viewModel.Add("test2", "apple");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.SubmitAndPay, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_DefaultBehaviour_WhenCheckboxContains_Condition_False()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { CheckboxContains = "invalid", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "wrongvalue");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.GoToExternalPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenCheckboxContains_Condition_True()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { CheckboxContains = "value", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "value");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMultipleCheckboxContains_Condition_True()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { CheckboxContains = "apple", QuestionId = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithCondition(new Condition { CheckboxContains = "pear", QuestionId = "test" })
                .Build();

            var behaviour3 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithCondition(new Condition { CheckboxContains = "berry", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "berry");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMultipleCheckboxContainsConditions_Condition_True()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { CheckboxContains = "apple", QuestionId = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithCondition(new Condition { CheckboxContains = "pear", QuestionId = "test", })
                .Build();

            var behaviour3 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithCondition(new Condition { CheckboxContains = "berry", QuestionId = "test" })
                .WithCondition(new Condition { CheckboxContains = "apple", QuestionId = "test", })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "berry,apple");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_CorrectBehaviour_When_MixedConditions_ForChecboxContains()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithCondition(new Condition { EqualTo = "berry", QuestionId = "test" })
                .Build();

            var behaviour3 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .WithCondition(new Condition { CheckboxContains = "mango", QuestionId = "test" })
                .Build();

            var behaviour4 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithCondition(new Condition { CheckboxContains = "pear", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .WithBehaviour(behaviour4)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "pear");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }


        [Fact]
        public void GetNextPage_ShouldReturn_CorrectBehaviour_When_MixedConditions_ForEqualTo()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { CheckboxContains = "apple", QuestionId = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithCondition(new Condition { CheckboxContains = "pear", QuestionId = "test" })
                .Build();

            var behaviour3 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .WithCondition(new Condition { EqualTo = "mango", QuestionId = "test" })
                .Build();

            var behaviour4 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithCondition(new Condition { EqualTo = "berry", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .WithBehaviour(behaviour4)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "berry");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_CorrectBehaviour_When_MixedConditions_Submit_And_CheckboxContains()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { CheckboxContains = "apple", QuestionId = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "apple");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_CorrectBehaviour_When_MixedConditions_Submit_And_EqualTo()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "apple");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_SubmitBehaviour()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { CheckboxContains = "berry", QuestionId = "test" })
                .Build();

            var behaviour3 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "pear");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }


        [Fact]
        public void GetNextPage_ShouldReturn_SubmitBehaviour_WhenMultipleSubmitForms()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("submit-one")
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("submit-two")
                .WithCondition(new Condition { CheckboxContains = "pear", QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "pear");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
            Assert.Equal("submit-two", result.PageSlug);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMix_OfEqualAndCheckbox_WithinSameBehaviour()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("submit-one")
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .WithCondition(new Condition { CheckboxContains = "berry", QuestionId = "data" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("submit-two")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "apple");
            viewModel.Add("data", "berry");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMix_OfMultipleEqualAndCheckbox_WithinSameBehaviour()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("submit-one")
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .WithCondition(new Condition { CheckboxContains = "berry", QuestionId = "data" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var behaviour3 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("submit-one")
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .WithCondition(new Condition { CheckboxContains = "pear", QuestionId = "data" })
                .WithCondition(new Condition { CheckboxContains = "berry", QuestionId = "data" })
                .Build();

            var behaviour4 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("submit-one")
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .WithCondition(new Condition { CheckboxContains = "berry", QuestionId = "data" })
                .WithCondition(new Condition { EqualTo = "data", QuestionId = "test2" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .WithBehaviour(behaviour4)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "apple");
            viewModel.Add("test2", "fish");
            viewModel.Add("data", "berry");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_BehaviourSubmit_WhenMix_OfMultipleEqualAndCheckbox_WithinSameBehaviour()
        {
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { EqualTo = "apple", QuestionId = "test" })
                .WithCondition(new Condition { CheckboxContains = "berry", QuestionId = "data" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "pear");
            viewModel.Add("data", "berry");

            var result = page.GetNextPage(viewModel);

            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
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

            var result = page.GetNextPage(new Dictionary<string, dynamic>()
            { { "testDate-year",DateTime.Today.Year.ToString() },
                { "testDate-month", DateTime.Today.Month.ToString() },
                { "testDate-day", DateTime.Today.Day.ToString() } }
            );

            Assert.Equal(PageSlugLessThan, result.PageSlug);

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

            var result = page.GetNextPage(new Dictionary<string, dynamic>()
            { { "testDate-year", futureDate.Year.ToString() },
                { "testDate-month", futureDate.Month.ToString() },
                { "testDate-day", futureDate.Day.ToString() } }
            );

            Assert.Equal(PageSlugGreaterThan, result.PageSlug);
        }

        [Fact]
        public void GetNextPage_ShouldGoToOther_WhenDateLessThan42DaysWithHtml5Control()
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

            var result = page.GetNextPage(
             new Dictionary<string, dynamic>() { { "testDate", DateTime.Today.ToString() } });

            Assert.Equal(PageSlugLessThan, result.PageSlug);
        }

        [Fact]
        public void GetNextPage_ShouldGoToOther_WhenDateGreaterThan42DaysWithHTML5Control()
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

            var result = page.GetNextPage(
           new Dictionary<string, dynamic>() { { "testDate", futureDate.ToString() } });

            Assert.Equal(PageSlugGreaterThan, result.PageSlug);
        }

        #endregion
    }
}
