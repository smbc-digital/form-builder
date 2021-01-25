using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class DatePickerTests
    {
        [Fact]
        public void GetDate_ShouldThrowException_IfDateIsIncorrectlyFormatted()
        {
            var viewModel = new Dictionary<string, dynamic>()
            {
                { "test-date", "ABC123" }
            };

            Assert.Throws<FormatException>(() => DatePicker.GetDate(viewModel, "test-date"));
        }

        [Fact]
        public void GetDate_ReturnCorrectDate()
        {
            var viewModel = new Dictionary<string, dynamic>()
            {
                { "test-date", "01/01/2020" }
            };

            var result =  DatePicker.GetDate(viewModel, "test-date");
            Assert.True(result.HasValue);
            Assert.True(result.Value == new DateTime(2020, 1, 1));
        }

        [Fact]
        public void GetDate_ReturnNull_IfKeyNotPresent()
        {
            var viewModel = new Dictionary<string, dynamic>();
            var result =  DatePicker.GetDate(viewModel, "test-date");
            Assert.False(result.HasValue);
        }

        [Fact]
        public void GetDate_ReturnNull_IfKeyPresentButValueIsEmpty()
        {
            var viewModel = new Dictionary<string, dynamic>()
            {
                { "test-date", "" } 
            };

            var result =  DatePicker.GetDate(viewModel, "test-date");
            Assert.False(result.HasValue);
        }

        [Fact]
        public void GetDate_WithFormAnswers_ReturnCorrectDate()
        {
            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers{
                        Answers = new List<Answers>
                        {
                            new Answers{
                                QuestionId = "test-date",
                                Response = "01/01/2020"
                            }
                            
                        }
                    }
                }
            };

            var result =  DatePicker.GetDate(formAnswers, "test-date");
            Assert.True(result.HasValue);
            Assert.True(result.Value == new DateTime(2020, 1, 1));
        }

        [Fact]
        public void GetDate_WithFormAnswers_ShouldThrowException_IfDateIsIncorrectlyFormatted()
        {
            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers{
                        Answers = new List<Answers>
                        {
                            new Answers{
                                QuestionId = "test-date",
                                Response = "ABC123"
                            }
                        }
                    }
                }
            };

            Assert.Throws<FormatException>(() => DatePicker.GetDate(formAnswers, "test-date"));
        }

        [Fact]
        public void GetDate_WithFormAnswers_ShouldReturnNull_IfAnswersIsNotPresent()
        {
            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers{ Answers = new List<Answers>() }
                }
            };

            Assert.False(DatePicker.GetDate(formAnswers, "test-date").HasValue);
        }

        [Fact]
        public void GetDate_WithFormAnswers_ShouldReturnNull_IfAnswersPresent_AndEmpty()
        {
            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers{
                        Answers = new List<Answers>
                        {
                            new Answers{ QuestionId = "test-date" }
                        }
                    }
                }
            };

            Assert.False(DatePicker.GetDate(formAnswers, "test-date").HasValue);
        }
    }
}