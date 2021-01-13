using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers
{
    public class FormAnswerTagParserTests
    {
        private readonly IEnumerable<IFormatter> _formatters;
        private readonly Mock<IFormatter> _mockFormatter = new Mock<IFormatter>();
        private readonly Mock<IFormatter> _mockFormatterTwo = new Mock<IFormatter>();

        private FormAnswerTagParser _tagParser;

        public FormAnswerTagParserTests()
        {
            _mockFormatter.Setup(_ => _.FormatterName).Returns("testformatter");
            _mockFormatter.Setup(_ => _.Parse(It.IsAny<string>())).Returns("FAKE-FORMATTED-VALUE");
            _mockFormatterTwo.Setup(_ => _.FormatterName).Returns("anothertestformatter");
            _mockFormatterTwo.Setup(_ => _.Parse(It.IsAny<string>())).Returns("ANOTHER-FORMATTER");
            _formatters = new List<IFormatter>
            {
                _mockFormatter.Object,
                _mockFormatterTwo.Object
            };

            _tagParser = new FormAnswerTagParser(_formatters);
        }

        [Theory]
        [InlineData("{{QUESTION:firstname}}")]
        [InlineData("{{QUESTION:ref}}")]
        public void Regex_ShouldReturnTrue_Result(string value)
        {
            Assert.True(_tagParser.Regex.Match(value).Success);
        }

        [Theory]
        [InlineData("{{QUESTIONN:firstname}}")]
        [InlineData("{{UESTIONN:firstname}}")]
        [InlineData("{QUESTIONN:firstname}")]
        [InlineData("{{QUESTIONN:firstname}")]
        [InlineData("{{TAG:firstname}")]
        [InlineData("{{DIFFERENTTAG:firstname}}")]
        public void Regex_ShouldReturnFalse_Result(string value)
        {
            Assert.False(_tagParser.Regex.Match(value).Success);
        }

        [Fact]
        public void Parse_ShouldReturnInitalValue_WhenNoValuesAre_To_BeReplaced()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this has no values to be replaced")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers();

            var result = _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
        }

        [Fact]
        public void Parse_ShouldReturnInitalValue_When_NoTag_MatchesRegex()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this value {{TAG:firstname}} should be replaced with name question")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers();

            var result = _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
        }

        [Fact]
        public void Parse_ShouldReturnUpdatedValue_WhenReplacingSingleValue()
        {
            var expectedString = "this value testfirstname should be replaced with name question";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value {{QUESTION:firstname}} should be replaced with name question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

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
                                QuestionId = "firstname",
                                Response = "testfirstname"
                            }
                        }
                    }
                }
            };

            var result = _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
        }

        [Fact]
        public void Parse_ShouldReturnUpdatedValue_WhenReplacingMultipleValues()
        {
            var expectedString = "this value testfirstname should be replaced with firstname and this testlastname with lastname";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value {{QUESTION:firstname}} should be replaced with firstname and this {{QUESTION:lastname}} with lastname")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

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
                                QuestionId = "firstname",
                                Response = "testfirstname"
                            },
                            new Answers
                            {
                                QuestionId = "lastname",
                                Response = "testlastname"
                            }
                        }
                    }
                }
            };

            var result = _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
        }


        [Fact]
        public void Parse_ShouldCallFormatter_WhenProvided()
        {
            var expectedString = "this value should be formatted: FAKE-FORMATTED-VALUE";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value should be formatted: {{QUESTION:firstname:testformatter}}")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

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
                                QuestionId = "firstname",
                                Response = "value"
                            }
                        }
                    }
                }
            };

            var result = _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            _mockFormatter.Verify(_ => _.Parse(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public void Parse_ShouldCall_Multiple_Formatters_WhenProvided()
        {
            var expectedString = "this value should be formatted: FAKE-FORMATTED-VALUE ANOTHER-FORMATTER";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value should be formatted: {{QUESTION:firstname:testformatter}} {{QUESTION:firstname:anothertestformatter}}")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

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
                                QuestionId = "firstname",
                                Response = "value"
                            }
                        }
                    }
                }
            };

            var result = _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            _mockFormatter.Verify(_ => _.Parse(It.IsAny<string>()), Times.Once);
            _mockFormatterTwo.Verify(_ => _.Parse(It.IsAny<string>()), Times.Once);
        }



        [Fact]
        public void Parse_ShouldCall_Same_Formatter_MultipleTimes()
        {
            var expectedString = "this value should be formatted: FAKE-FORMATTED-VALUE FAKE-FORMATTED-VALUE";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value should be formatted: {{QUESTION:firstname:testformatter}} {{QUESTION:firstname:testformatter}}")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

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
                                QuestionId = "firstname",
                                Response = "value"
                            }
                        }
                    }
                }
            };

            var result = _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            _mockFormatter.Verify(_ => _.Parse(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}