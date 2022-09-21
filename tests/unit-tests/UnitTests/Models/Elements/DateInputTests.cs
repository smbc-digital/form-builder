using form_builder.Models;
using form_builder.Models.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class DateInputTests
    {
        [Fact]
        public void GetDate_ReturnCorrectDate()
        {
            var viewModel = new Dictionary<string, dynamic>
            {
                { $"test-date{DateInput.YEAR_EXTENSION}", "2020" },
                { $"test-date{DateInput.MONTH_EXTENSION}", "01" },
                { $"test-date{DateInput.DAY_EXTENSION}", "01" }
            };

            DateTime? result = DateInput.GetDate(viewModel, "test-date");
            Assert.True(result.HasValue);
            Assert.True(result.Value == new DateTime(2020, 1, 1));
        }
        [Fact]
        public void GetDate_ReturnNull_IfOnlyPartialCorrectDate()
        {
            var viewModel = new Dictionary<string, dynamic>
            {
                { $"test-date{DateInput.YEAR_EXTENSION}", "2020" },
                { $"test-date{DateInput.MONTH_EXTENSION}", "01" },
            };

            var result = DateInput.GetDate(viewModel, "test-date");
            Assert.False(result.HasValue);
        }

        [Fact]
        public void GetDate_ReturnNull_IfKeyNotPresent()
        {
            var viewModel = new Dictionary<string, dynamic>();
            DateTime? result = DateInput.GetDate(viewModel, "test-date");

            Assert.False(result.HasValue);
        }

        [Fact]
        public void GetDate_WithFormAnswers_ReturnCorrectDate()
        {
            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = $"test-date{DateInput.YEAR_EXTENSION}",
                                Response = "2020"
                            },
                            new Answers
                            {
                                QuestionId = $"test-date{DateInput.MONTH_EXTENSION}",
                                Response = "01"
                            },
                            new Answers
                            {
                                QuestionId = $"test-date{DateInput.DAY_EXTENSION}",
                                Response = "01"
                            }
                        }
                    }
                }
            };

            DateTime? result = DateInput.GetDate(formAnswers, "test-date");
            Assert.True(result.HasValue);
            Assert.True(result.Value == new DateTime(2020, 1, 1));
        }
    }
}