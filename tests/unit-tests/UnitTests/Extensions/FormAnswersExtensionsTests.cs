using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class FormAnswersExtensionsTests
    {
        [Fact]
        public void GetReducedAnswers_ShouldRemoveUnusedPage()
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
                    },
                    new PageAnswers
                    {
                        Answers = new List<Answers>(),
                        PageSlug = "page-3"
                    }
                }
            };

            var formSchema = new FormSchema
            {
                Pages = new List<Page>
                {
                    new Page
                    {
                        PageSlug = "page-1",
                        Behaviours = new List<Behaviour>
                        {
                            new Behaviour
                            {
                                BehaviourType = EBehaviourType.GoToPage,
                                PageSlug = "page-3"
                            }
                        }
                    },
                    new Page
                    {
                        PageSlug = "page-2",
                        Behaviours = new List<Behaviour>
                        {
                            new Behaviour
                            {
                                BehaviourType = EBehaviourType.GoToPage,
                                PageSlug = "page-3"
                            }
                        }
                    },
                    new Page
                    {
                        PageSlug = "page-3",
                        Behaviours = new List<Behaviour>
                        {
                            new Behaviour
                            {
                                BehaviourType = EBehaviourType.SubmitForm
                            }
                        }
                    }
                },
                FirstPageSlug = "page-1"
            };

            // Act
            var result = formAnswers.GetReducedAnswers(formSchema);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetReducedAnswers_ShouldReturnPageAnswers_ThatExistInSchema()
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
                    },
                    new PageAnswers
                    {
                        Answers = new List<Answers>(),
                        PageSlug = "page-3"
                    }
                }
            };

            var formSchema = new FormSchemaBuilder()
                .WithPage(new Page
                {
                    PageSlug = "page-1",
                    Behaviours = new List<Behaviour>
                    {
                        new Behaviour
                        {
                            BehaviourType = EBehaviourType.GoToPage,
                            PageSlug = "page-3"
                        }
                    }
                })
                .WithPage(new Page
                {
                    PageSlug = "page-2",
                    Behaviours = new List<Behaviour>
                        {
                            new Behaviour
                            {
                                BehaviourType = EBehaviourType.GoToPage,
                                PageSlug = "page-3"
                            }
                        }
                }
                )
                .WithFirstPageSlug("page-1")
                .Build();

            // Act
            var result = formAnswers.GetReducedAnswers(formSchema);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void GetReducedAnswers_ShouldReturnPageAnswers_ThatExistInAnswers()
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

            var formSchema = new FormSchemaBuilder()
                .WithPage(new Page
                {
                    PageSlug = "page-1",
                    Behaviours = new List<Behaviour>
                    {
                        new Behaviour
                        {
                            BehaviourType = EBehaviourType.GoToPage,
                            PageSlug = "page-3"
                        }
                    }
                })
                .WithPage(new Page
                {
                    PageSlug = "page-2",
                    Behaviours = new List<Behaviour>
                    {
                        new Behaviour
                        {
                            BehaviourType = EBehaviourType.GoToPage,
                            PageSlug = "page-3"
                        }
                    }
                })
                .WithPage(new Page
                {
                    PageSlug = "page-3",
                    Behaviours = new List<Behaviour>
                    {
                        new Behaviour
                        {
                            BehaviourType = EBehaviourType.SubmitForm
                        }
                    }
                })
                .WithFirstPageSlug("page-1")
                .Build();

            // Act
            var result = formAnswers.GetReducedAnswers(formSchema);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void GetReducedAnswers_ShouldReturnCorrectPages_IfNoAnswersGiven()
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
                                QuestionId = "test1"
                            }
                        }
                    },
                    new PageAnswers
                    {
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = "test"
                            }
                        },
                        PageSlug = "page-2"
                    }
                }
            };

            var formSchema = new FormSchemaBuilder()
                .WithPage(new Page
                {
                    PageSlug = "page-1",
                    Behaviours = new List<Behaviour>
                    {
                        new Behaviour
                        {
                            BehaviourType = EBehaviourType.GoToPage,
                            PageSlug = "page-2"
                        }
                    }
                })
                .WithPage(new Page
                {
                    PageSlug = "page-2",
                    Behaviours = new List<Behaviour>
                    {
                        new Behaviour
                        {
                            BehaviourType = EBehaviourType.SubmitForm
                        }
                    }
                })
                .WithFirstPageSlug("page-1")
                .Build();

            // Act
            var result = formAnswers.GetReducedAnswers(formSchema);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Theory]
        [InlineData("question2", "yes", "yes-routing-answer")]
        [InlineData("question3", "no", "no-routing-answer")]
        public void GetReducedAnswers_ShouldReturnCorrectAnswers_WhenScheamContains_TwoOfSamePageSlug_WithRenderConditions(string questionId, string answer, string value)
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
            var result = formAnswers.GetReducedAnswers(formSchema);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(questionId, result[1].Answers[0].QuestionId);
            Assert.Equal(value, result[1].Answers[0].Response);
        }

        [Fact]
        public void GetReducedAnswers_ShouldReturnCorrectAnswers_WhenScheamContains_TwoOfSamePageSlug_WithRenderConditions_AndOptionalPage()
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
                                Response = "no"
                            }
                        }
                    },
                    new PageAnswers
                    {
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = "question2",
                                Response = "answer"
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
                .WithQuestionId("routingquestion")
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
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "yes"
                })
                .Build();

            var behaviour3 = new BehaviourBuilder()
                .WithPageSlug("page-4")
                .WithBehaviourType(EBehaviourType.GoToPage)
                .Build();

            var page3 = new PageBuilder()
                .WithPageSlug("page-2")
                .WithBehaviour(behaviour3)
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "no"
                })
                .Build();

            var page4 = new PageBuilder()
                .WithPageSlug("page-4")
                .WithBehaviour(behaviour2)
                .WithElement(element3)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(page2)
                .WithPage(page3)
                .WithPage(page4)
                .WithFirstPageSlug("page-1")
                .Build();

            // Act
            var result = formAnswers.GetReducedAnswers(formSchema);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("routingquestion", result[2].Answers[0].QuestionId);
            Assert.Equal("data", result[2].Answers[0].Response);
        }
    }
}
