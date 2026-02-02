using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers
{
    public class FormAnswerTagParserTests
    {
        private readonly Mock<IFormatter> _mockFormatter = new();
        private readonly Mock<IFormatter> _mockFormatterTwo = new();

        private readonly FormAnswerTagParser _tagParser;

        public FormAnswerTagParserTests()
        {
            _mockFormatter.Setup(mock => mock.FormatterName).Returns("testformatter");
            _mockFormatter.Setup(mock => mock.Parse(It.IsAny<string>())).Returns("FAKE-FORMATTED-VALUE");
            _mockFormatterTwo.Setup(mock => mock.FormatterName).Returns("anothertestformatter");
            _mockFormatterTwo.Setup(mock => mock.Parse(It.IsAny<string>())).Returns("ANOTHER-FORMATTER");
            IEnumerable<IFormatter> formatters = new List<IFormatter>
            {
                _mockFormatter.Object,
                _mockFormatterTwo.Object
            };

            _tagParser = new FormAnswerTagParser(formatters);
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
        public async Task Parse_ShouldReturnInitialValue_WhenNoValuesAre_To_BeReplaced()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this has no values to be replaced")
                .Build();

            var ulElement = new ElementBuilder()
                .WithType(EElementType.UL)
                .WithListItems(["this item has no values to be replaced"])
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(ulElement)
                .Build();

            var formAnswers = new FormAnswers();

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
            Assert.Equal(ulElement.Properties.ListItems.First(), result.Elements.First(element => element.Type.Equals(EElementType.UL)).Properties.ListItems.First());
        }

        [Fact]
        public async Task Parse_ShouldReturnInitialValue_When_NoTag_MatchesRegex()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this value {{TAG:firstname}} should be replaced with name question")
                .Build();

            var ulElement = new ElementBuilder()
                .WithType(EElementType.UL)
                .WithListItems(["this value {{TAG:firstname}} should be replaced with name question"])
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(ulElement)
                .Build();

            var formAnswers = new FormAnswers();

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
            Assert.Equal(ulElement.Properties.ListItems.First(), result.Elements.First(element => element.Type.Equals(EElementType.UL)).Properties.ListItems.First());
        }

        [Fact]
        public async Task Parse_ShouldReturnUpdatedValue_WhenReplacingSingleValue()
        {
            var expectedString = "this value testfirstname should be replaced with name question";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value {{QUESTION:firstname}} should be replaced with name question")
               .Build();

            var ulElement = new ElementBuilder()
                .WithType(EElementType.UL)
                .WithListItems(["this value {{QUESTION:firstname}} should be replaced with name question"])
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(ulElement)
                .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "testfirstname"
                            }
                        }
                    }
                }
            };

            var result = await _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            Assert.Equal(expectedString, result.Elements.First(element => element.Type.Equals(EElementType.UL)).Properties.ListItems.First());
        }

        [Fact]
        public async Task Parse_ShouldReturnUpdatedValueForHint_WhenReplacingSingleValue()
        {
            var expectedString = "this value testfirstname should be replaced with name question";

            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithHint("this value {{QUESTION:firstname}} should be replaced with name question")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "testfirstname"
                            }
                        }
                    }
                }
            };

            var result = await _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Hint);
        }

        [Fact]
        public async Task Parse_ShouldReturnUpdatedValueForLimitNextAvailableFromDate_WhenReplacingSingleValue()
        {
            var expectedString = "01-01-2022";

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithLimitNextAvailableFromDate("{{QUESTION:date}}")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "date",
                                Response = "01-01-2022"
                            }
                        }
                    }
                }
            };

            var result = await _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.LimitNextAvailableFromDate);
        }

        [Fact]
        public async Task Parse_ShouldReturnUpdatedValue_WhenReplacingMultipleValues()
        {
            var expectedString = "this value testfirstname should be replaced with firstname and this testlastname with lastname";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value {{QUESTION:firstname}} should be replaced with firstname and this {{QUESTION:lastname}} with lastname")
               .Build();

            var ulElement = new ElementBuilder()
                .WithType(EElementType.UL)
                .WithListItems(["this value {{QUESTION:firstname}} should be replaced with firstname and this {{QUESTION:lastname}} with lastname"])
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(ulElement)
                .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "testfirstname"
                            },
                            new()
                            {
                                QuestionId = "lastname",
                                Response = "testlastname"
                            }
                        }
                    }
                }
            };

            var result = await _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            Assert.Equal(expectedString, result.Elements.First(element => element.Type.Equals(EElementType.UL)).Properties.ListItems.First());
        }

        [Fact]
        public async Task Parse_ShouldReturnUpdatedValueForHint_WhenReplacingMultipleValues()
        {
            var expectedString = "this value testfirstname should be replaced with firstname and this testlastname with lastname";

            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithHint("this value {{QUESTION:firstname}} should be replaced with firstname and this {{QUESTION:lastname}} with lastname")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "testfirstname"
                            },
                            new()
                            {
                                QuestionId = "lastname",
                                Response = "testlastname"
                            }
                        }
                    }
                }
            };

            var result = await _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Hint);
        }

        [Fact]
        public async Task Parse_ShouldCallFormatter_WhenProvided()
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
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "value"
                            }
                        }
                    }
                }
            };

            var result = await _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            _mockFormatter.Verify(mock => mock.Parse(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task Parse_ShouldCall_Multiple_Formatters_WhenProvided()
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
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "value"
                            }
                        }
                    }
                }
            };

            var result = await _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            _mockFormatter.Verify(mock => mock.Parse(It.IsAny<string>()), Times.Once);
            _mockFormatterTwo.Verify(mock => mock.Parse(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Parse_ShouldCall_Same_Formatter_MultipleTimes()
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
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "value"
                            }
                        }
                    }
                }
            };

            var result = await _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            _mockFormatter.Verify(mock => mock.Parse(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public void ParseString_ShouldReturnInitialValue_WhenNoValuesAre_To_BeReplaced()
        {
            var text = "this has no values to be replaced";
            var formAnswers = new FormAnswers();

            var result = _tagParser.ParseString(text, formAnswers);

            Assert.Equal(text, result);
        }

        [Fact]
        public void ParseString_ShouldReturnInitialValue_When_NoTag_MatchesRegex()
        {
            var text = "this value {{TAG:firstname}} should be replaced with name question";
            var formAnswers = new FormAnswers();

            var result = _tagParser.ParseString(text, formAnswers);

            Assert.Equal(text, result);
        }

        [Fact]
        public async Task Parse_ShouldReturnInitialValueForLeadingParagraph_When_NoTag_MatchesRegex()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this value {{TAG}} should be replaced with value")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithLeadingParagraph("this value {{TAG}} should be replaced with value")
                .Build();

            var formAnswers = new FormAnswers();

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
            Assert.Equal(page.LeadingParagraph, result.LeadingParagraph);
        }

        [Fact]
        public async Task Parse_ShouldReturnUpdatedValueForLeadingParagraph_WhenReplacingSingleValue()
        {
            var expectedString = "this value firstname should be replaced with firstname";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value {{QUESTION:firstname}} should be replaced with firstname")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithLeadingParagraph("this value {{QUESTION:firstname}} should be replaced with firstname")
                .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "firstname"
                            }
                        }
                    }
                }
            };

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            Assert.Equal(expectedString, result.LeadingParagraph);
        }

        [Fact]
        public async Task Parse_ShouldReturnUpdatedValueForLeadingParagraph_WhenReplacingMultipleValues()
        {
            var expectedString = "this value firstname should be replaced with firstname and this lastname with lastname";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value {{QUESTION:firstname}} should be replaced with firstname and this {{QUESTION:lastname}} with lastname")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithLeadingParagraph("this value {{QUESTION:firstname}} should be replaced with firstname and this {{QUESTION:lastname}} with lastname")
                .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "firstname"
                            },
                            new()
                            {
                                QuestionId = "lastname",
                                Response = "lastname"
                            }
                        }
                    }
                }
            };            
            
            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            Assert.Equal(expectedString, result.LeadingParagraph);
        }

        [Fact]
        public void ParseString_ShouldReturnUpdatedValue_WhenReplacingSingleValue()
        {
            var expectedString = "this value testfirstname should be replaced with name question";
            var text = "this value {{QUESTION:firstname}} should be replaced with name question";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "testfirstname"
                            }
                        }
                    }
                }
            };

            var result = _tagParser.ParseString(text, formAnswers);
            Assert.Equal(expectedString, result);
        }

        [Fact]
        public void ParseString_ShouldReturnUpdatedValue_WhenReplacingMultipleValues()
        {
            var expectedString = "this value testfirstname should be replaced with firstname and this testlastname with lastname";
            var text = "this value {{QUESTION:firstname}} should be replaced with firstname and this {{QUESTION:lastname}} with lastname";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "testfirstname"
                            },
                            new()
                            {
                                QuestionId = "lastname",
                                Response = "testlastname"
                            }
                        }
                    }
                }
            };

            var result = _tagParser.ParseString(text, formAnswers);
            Assert.Equal(expectedString, result);
        }


        [Fact]
        public void ParseString_ShouldCallFormatter_WhenProvided()
        {
            var expectedString = "this value should be formatted: FAKE-FORMATTED-VALUE";
            var text = "this value should be formatted: {{QUESTION:firstname:testformatter}}";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "value"
                            }
                        }
                    }
                }
            };

            var result = _tagParser.ParseString(text, formAnswers);
            Assert.Equal(expectedString, result);
            _mockFormatter.Verify(mock => mock.Parse(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public void ParseString_ShouldCall_Multiple_Formatters_WhenProvided()
        {
            var expectedString = "this value should be formatted: FAKE-FORMATTED-VALUE ANOTHER-FORMATTER";
            var text = "this value should be formatted: {{QUESTION:firstname:testformatter}} {{QUESTION:firstname:anothertestformatter}}";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "value"
                            }
                        }
                    }
                }
            };

            var result = _tagParser.ParseString(text, formAnswers);
            Assert.Equal(expectedString, result);
            _mockFormatter.Verify(mock => mock.Parse(It.IsAny<string>()), Times.Once);
            _mockFormatterTwo.Verify(mock => mock.Parse(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ParseString_ShouldCall_Same_Formatter_MultipleTimes()
        {
            var expectedString = "this value should be formatted: FAKE-FORMATTED-VALUE FAKE-FORMATTED-VALUE";
            var text = "this value should be formatted: {{QUESTION:firstname:testformatter}} {{QUESTION:firstname:testformatter}}";

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new()
                    {
                        Answers = new List<Answers>
                        {
                            new()
                            {
                                QuestionId = "firstname",
                                Response = "value"
                            }
                        }
                    }
                }
            };

            var result = _tagParser.ParseString(text, formAnswers);
            Assert.Equal(expectedString, result);
            _mockFormatter.Verify(mock => mock.Parse(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}