using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using System.Collections.Generic;
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
    }
}
