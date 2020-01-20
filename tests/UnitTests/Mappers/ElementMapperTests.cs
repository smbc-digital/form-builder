using form_builder.Mappers;
using form_builder.Models;
using form_builder_tests.Builders;
using System;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Mappers
{
    public class ElementMapperTests
    {
        [Fact]
        public void GetAnswerValue_ShouldReturnIntWhenNumericIsTrue()
        {
            var element = new ElementBuilder()
            .WithNumeric(true)
            .WithQuestionId("testNumber")
            .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "testNumber",
                                Response = "23"
                            }
                        }
                    }
                }
            };

            var result = ElementMapper.GetAnswerValue(element, formAnswers);

            Assert.IsType<int>(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnSingleDateWhenElementIsDatePicker()
        {
            var element = new ElementBuilder()
            .WithType(form_builder.Enum.EElementType.DatePicker)
            .WithQuestionId("testDate")
            .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "testDate",
                                Response = "21/02/2020"
                            }
                        }
                    }
                }
            };

            var result = ElementMapper.GetAnswerValue(element, formAnswers);

            Assert.IsType<DateTime>(result);
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNullWhenResponseIsEmpty_WhenElementIsDatePicker()
        {
            {
                var element = new ElementBuilder()
                .WithType(form_builder.Enum.EElementType.DatePicker)
                .WithQuestionId("testNumber")
                .Build();

                var formAnswers = new FormAnswers
                {
                    Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "testNumber"
                            }
                        }
                    }
                }
                };

                var result = ElementMapper.GetAnswerValue(element, formAnswers);

                Assert.Null(result);
            }
        }

        [Fact]
        public void GetAnswerValue_ShouldReturnNullWhenResponseIsEmpty_AndNumeric()
        {
            {
                var element = new ElementBuilder()
                .WithQuestionId("testNumber")
                .WithNumeric(true)
                .Build();

                var formAnswers = new FormAnswers
                {
                    Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                        Answers = new List<Answers> {
                            new Answers
                            {
                                QuestionId = "testNumber"
                            }
                        }
                    }
                }
                };

                var result = ElementMapper.GetAnswerValue(element, formAnswers);

                Assert.Null(result);
            }
        }
    }
}
