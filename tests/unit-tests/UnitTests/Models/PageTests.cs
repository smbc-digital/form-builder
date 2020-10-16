using System;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Models
{
    public class PageTests
    {
        [Fact]
        public void GetSubmitFormEndpoint_ShouldReturnPageSlug_WhenOnlyOneBehaviourExistsAndNoSubmitSlugs()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("page-one")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            // Act
            var result = page.GetSubmitFormEndpoint(new FormAnswers(), "");

            // Assert
            Assert.Equal("page-one", result.URL);
        }

        [Fact]
        public void GetSubmitFormEndpoint_ShouldReturnNull_WhenNoFormSubmitAction()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-one")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => page.GetSubmitFormEndpoint(new FormAnswers(), null));
        }

        [Fact]
        public void GetSubmitFormEndpoint_ShouldReturnCorrectSubmitActionUrl_WhenMultipleSubmits()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("page-one")
                .WithSubmitSlug(new SubmitSlug { Environment = "test" })
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("page-two")
                .WithCondition(new Condition { EqualTo = "test", QuestionId = "test" })
                .WithSubmitSlug(new SubmitSlug { Environment = "test", URL = "page-two" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            // Act
            var result = page.GetSubmitFormEndpoint(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        PageSlug = "page-one",
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = "test",
                                Response = "test"
                            }
                        }
                    }
                }
            }, "test");

            // Assert
            Assert.Equal("page-two", result.URL);
        }

        [Theory]
        [InlineData(EBehaviourType.GoToPage)]
        [InlineData(EBehaviourType.GoToExternalPage)]
        [InlineData(EBehaviourType.SubmitAndPay)]
        [InlineData(EBehaviourType.SubmitForm)]
        public void GetNextPage_ShouldReturn_DefaultBehaviour(EBehaviourType type)
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(type)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            // Act
            var result = page.GetNextPage(new Dictionary<string, dynamic>());

            // Assert
            Assert.Equal(type, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_DefaultBehaviour_WhenEqualTo_Condition_False()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "wrongValue"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.GoToExternalPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenEqualTo_Condition_True()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "value"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMultipleEqualTo_Condition_True()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "pear"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.SubmitAndPay, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMultipleEqualToConditions_Condition_True()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test", "pear"},
                {"test2", "apple"}
            };

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.SubmitAndPay, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_DefaultBehaviour_WhenCheckboxContains_Condition_False()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "wrongValue"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.GoToExternalPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenCheckboxContains_Condition_True()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "value"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMultipleCheckboxContains_Condition_True()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "berry"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMultipleCheckboxContainsConditions_Condition_True()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "berry,apple"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_CorrectBehaviour_When_MixedConditions_ForCheckboxContains()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "pear"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }


        [Fact]
        public void GetNextPage_ShouldReturn_CorrectBehaviour_When_MixedConditions_ForEqualTo()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "berry"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_CorrectBehaviour_When_MixedConditions_Submit_And_CheckboxContains()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "apple"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_CorrectBehaviour_When_MixedConditions_Submit_And_EqualTo()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "apple"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_SubmitBehaviour()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "pear"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_SubmitBehaviour_WhenMultipleSubmitForms()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "pear"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
            Assert.Equal("submit-two", result.PageSlug);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMix_OfEqualAndCheckbox_WithinSameBehaviour()
        {
            // Act
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

            var viewModel = new Dictionary<string, dynamic> {{"test", "apple"}, {"data", "berry"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenMix_OfMultipleEqualAndCheckbox_WithinSameBehaviour()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test", "apple"},
                {"test2", "fish"},
                {"data", "berry"}
            };

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_BehaviourSubmit_WhenMix_OfMultipleEqualAndCheckbox_WithinSameBehaviour()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic>
            {
                {"test", "pear"},
                {"data", "berry"}
            };

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.SubmitForm, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldGoToOther_WhenDateLessThan42Days()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("less-than")
                .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsBefore = 42, Unit = EDateUnit.Day, QuestionId = "testDate" })
                .Build();

            var behaviour3 = new BehaviourBuilder()
               .WithBehaviourType(EBehaviourType.GoToPage)
               .WithPageSlug("more-than")
               .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsAfter = 43, Unit = EDateUnit.Day, QuestionId = "testDate" })
               .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            // Act
            var result = page.GetNextPage(new Dictionary<string, dynamic>
            { 
                { "testDate-year",DateTime.Today.Year.ToString() },
                { "testDate-month", DateTime.Today.Month.ToString() },
                { "testDate-day", DateTime.Today.Day.ToString() } }
            );

            // Assert
            Assert.Equal("less-than", result.PageSlug);
        }

        [Fact]
        public void GetNextPage_ShouldGoToOther_WhenDateGreaterThan42Days()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("less-than")
                .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsBefore = 42, Unit = EDateUnit.Day, QuestionId = "testDate" })
                .Build();

            var behaviour3 = new BehaviourBuilder()
               .WithBehaviourType(EBehaviourType.GoToPage)
               .WithPageSlug("more-than")
               .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsAfter = 43, Unit = EDateUnit.Day, QuestionId = "testDate" })
               .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var futureDate = DateTime.Today.AddDays(50);

            // Act
            var result = page.GetNextPage(new Dictionary<string, dynamic>
            {
                { "testDate-year", futureDate.Year.ToString() },
                { "testDate-month", futureDate.Month.ToString() },
                { "testDate-day", futureDate.Day.ToString() } }
            );

            // Assert
            Assert.Equal("more-than", result.PageSlug);
        }

        [Fact]
        public void GetNextPage_ShouldGoToOther_WhenDateLessThan42DaysWithHtml5Control()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("less-than")
                .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsBefore = 42, Unit = EDateUnit.Day, QuestionId = "testDate" })
                .Build();

            var behaviour3 = new BehaviourBuilder()
               .WithBehaviourType(EBehaviourType.GoToPage)
               .WithPageSlug("more-than")
               .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsAfter = 43, Unit = EDateUnit.Day, QuestionId = "testDate" })
               .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            // Act
            var result = page.GetNextPage(
             new Dictionary<string, dynamic> { { "testDate", DateTime.Today.ToString() } });

            // Assert
            Assert.Equal("less-than", result.PageSlug);
        }

        [Fact]
        public void GetNextPage_ShouldGoToOther_WhenDateGreaterThan42DaysWithHTML5Control()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("less-than")
                .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsBefore = 42, Unit = EDateUnit.Day, QuestionId = "testDate" })
                .Build();


            var behaviour3 = new BehaviourBuilder()
               .WithBehaviourType(EBehaviourType.GoToPage)
               .WithPageSlug("more-than")
               .WithCondition(new Condition { ComparisonDate = DateTime.Today.ToString("yyyy-MM-dd"), IsAfter = 43, Unit = EDateUnit.Day, QuestionId = "testDate" })
               .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .WithBehaviour(behaviour3)
                .Build();

            var futureDate = DateTime.Today.AddDays(50);

            // Act
            var result = page.GetNextPage(
           new Dictionary<string, dynamic> { { "testDate", futureDate.ToString() } });
            
            // Assert
            Assert.Equal("more-than", result.PageSlug);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenIsNullOrEmpty_Condition_True()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { IsNullOrEmpty = true, QuestionId = "test" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic> {{"test", "value"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.GoToExternalPage, result.BehaviourType);
        }

        [Fact]
        public void GetNextPage_ShouldReturn_Behaviour_WhenIsNullOrEmpty_Condition_False()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToExternalPage)
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(new Condition { IsNullOrEmpty = false, QuestionId = "test", ConditionType = ECondition.IsNullOrEmpty })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var viewModel = new Dictionary<string, dynamic> {{"test", "value"}};

            // Act
            var result = page.GetNextPage(viewModel);

            // Assert
            Assert.Equal(EBehaviourType.GoToPage, result.BehaviourType);
        }

        [Fact]
        public void CheckPageMeetsConditions_ShouldReturnTrue_If_RenderConditionsAreValid()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithRenderConditions(new Condition
                {
                    QuestionId = "testRadio",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "yes"
                })
                .Build();

            var viewModel = new Dictionary<string, dynamic> { { "testRadio", "yes" } };

            // Act
            var result = page.CheckPageMeetsConditions(viewModel);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckPageMeetsConditions_ShouldReturnTrue_If_NoRenderConditionsExist()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var viewModel = new Dictionary<string, dynamic> { { "testRadio", "yes" } };

            // Act
            var result = page.CheckPageMeetsConditions(viewModel);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckPageMeetsConditions_ShouldReturnFalse_If_RenderConditionsAreNotValid()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-continue")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithRenderConditions(new Condition
                {
                    QuestionId = "testRadio",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "yes"
                })
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = page.CheckPageMeetsConditions(viewModel);

            // Assert
            Assert.False(result);
        }
    }
}
