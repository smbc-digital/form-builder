using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class FormSchemaExtensionsTests
    {
        [Fact]
        public void GetReducedPages_ShouldReturnOnlyPagesForUsersJourney()
        {
            // Arrange
            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        Answers = new List<Answers>(),
                        PageSlug = "page-1"
                    },
                    new PageAnswers
                    {
                        Answers = new List<Answers>(),
                        PageSlug = "page-2"
                    }
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            var behaviour1 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("summary")
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour1)
                .WithPageSlug("page-1")
                .Build();

            var pageBehaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("summary")
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(pageBehaviour)
                .WithPageSlug("page-2")
                .Build();

            var summaryElement = new ElementBuilder()
                .WithType(EElementType.Summary)
                .Build();

            var submitBehaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithPageSlug("success")
                .Build();

            var summaryPage = new PageBuilder()
                .WithElement(summaryElement)
                .WithBehaviour(submitBehaviour)
                .WithPageSlug("summary")
                .Build();

            var successPage = new PageBuilder()
                .WithPageSlug("success")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page1)
                .WithPage(page2)
                .WithPage(summaryPage)
                .WithPage(successPage)
                .WithFirstPageSlug("page-1")
                .Build();

            // Act
            var result = formSchema.GetReducedPages(formAnswers);
            var page1Result = result.Where(_ => _.PageSlug.Equals("page-1"));
            var summaryResult = result.Where(_ => _.PageSlug.Equals("summary"));
            var successResult = result.Where(_ => _.PageSlug.Equals("success"));

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Single(page1Result);
            Assert.Single(summaryResult);
            Assert.Single(successResult);
        }

        [Theory]
        [InlineData("question2", "yes", "yes-routing-answer")]
        [InlineData("question3", "no", "no-routing-answer")]
        public void GetReducedAnswers_ShouldReturnCorrectPages_WhenSchemaContains_TwoOfSamePageSlug_WithRenderConditions(string questionId, string answer, string value)
        {
            // Arrange
            var formAnswers = new FormAnswers
            {
                FormName = "test",
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        PageSlug = "page-1",
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = "test",
                                Response = answer
                            }
                        }
                    },
                    new PageAnswers
                    {
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = questionId,
                                Response = value
                            }
                        },
                        PageSlug = "page-2"
                    },
                    new PageAnswers
                    {
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = "routingquestion",
                                Response = "data"
                            }
                        },
                        PageSlug = "page-4"
                    }
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("test")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("question2")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("question3")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithPageSlug("page-2")
                .WithBehaviourType(EBehaviourType.GoToPage)
                .Build();

            var page = new PageBuilder()
                .WithPageSlug("page-1")
                .WithElement(element)
                .WithBehaviour(behaviour)
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var page2 = new PageBuilder()
                .WithPageSlug("page-2")
                .WithBehaviour(behaviour2)
                .WithElement(element2)
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "yes"
                })
                .Build();

            var page3 = new PageBuilder()
                .WithPageSlug("page-2")
                .WithBehaviour(behaviour2)
                .WithElement(element3)
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "no"
                })
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(page2)
                .WithPage(page3)
                .WithFirstPageSlug("page-1")
                .Build();

            // Act
            var result = formSchema.GetReducedPages(formAnswers);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(questionId, result[1].Elements[0].Properties.QuestionId);
        }
    }
}
