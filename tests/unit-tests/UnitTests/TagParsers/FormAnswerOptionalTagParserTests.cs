using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers;

public class FormAnswerOptionalTagParserTests
{
    private readonly Mock<IFormatter> _mockFormatter = new();
    private readonly Mock<IFormatter> _mockFormatterTwo = new();

    private readonly FormAnswerOptionalTagParser _tagParser;

    public FormAnswerOptionalTagParserTests()
    {
        _mockFormatter.Setup(x => x.FormatterName).Returns("testformatter");
        _mockFormatter.Setup(x => x.Parse(It.IsAny<string>()))
            .Returns("FAKE-FORMATTED-VALUE");

        _mockFormatterTwo.Setup(x => x.FormatterName).Returns("anothertestformatter");
        _mockFormatterTwo.Setup(x => x.Parse(It.IsAny<string>()))
            .Returns("ANOTHER-FORMATTER");

        IEnumerable<IFormatter> formatters = new List<IFormatter>
        {
            _mockFormatter.Object,
            _mockFormatterTwo.Object
        };

        _tagParser = new FormAnswerOptionalTagParser(formatters);
    }

    [Theory]
    [InlineData("{{QUESTIONOPT:firstname}}")]
    [InlineData("{{QUESTIONOPT:ref}}")]
    public void Regex_ShouldReturnTrue_Result(string value) => Assert.True(_tagParser.Regex.Match(value).Success);

    [Theory]
    [InlineData("{{QUESTION:firstname}}")]
    [InlineData("{{QUESTIONOPT}}")]
    [InlineData("{QUESTIONOPT:firstname}")]
    [InlineData("{{QUESTIONOPT:firstname}")]
    [InlineData("{{DIFFERENTTAG:firstname}}")]
    public void Regex_ShouldReturnFalse_Result(string value) => Assert.False(_tagParser.Regex.Match(value).Success);

    [Fact]
    public async Task Parse_ShouldReturnInitialValue_WhenNoValues()
    {
        Element element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithPropertyText("no values")
            .Build();

        Page page = new PageBuilder()
            .WithElement(element)
            .Build();

        Page result = await _tagParser.Parse(page, new FormAnswers());

        Assert.Equal("no values", result.Elements.First().Properties.Text);
    }

    [Fact]
    public async Task Parse_ShouldReturnUpdatedValue_WhenReplacingSingleValue()
    {
        string expected = "value test";

        Element element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithPropertyText("value {{QUESTIONOPT:q1}}")
            .Build();

        Page page = new PageBuilder().WithElement(element).Build();

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
                            QuestionId = "q1",
                            Response = "test"
                        }
                    }
                }
            }
        };

        Page result = await _tagParser.Parse(page, formAnswers);

        Assert.Equal(expected, result.Elements.First().Properties.Text);
    }

    [Fact]
    public async Task Parse_ShouldRemoveTag_WhenValueMissing_Optional()
    {
        Element element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithPropertyText("value {{QUESTIONOPT:q1}}")
            .Build();

        Page page = new PageBuilder().WithElement(element).Build();

        var formAnswers = new FormAnswers
        {
            Pages = new List<PageAnswers>()
        };

        Page result = await _tagParser.Parse(page, formAnswers);

        Assert.Equal("value ", result.Elements.First().Properties.Text);
    }

    [Fact]
    public async Task Parse_ShouldUpdate_All_PropertyTypes()
    {
        Element element = new ElementBuilder()
            .WithType(EElementType.Booking)
            .WithPropertyText("{{QUESTIONOPT:q1}}")
            .WithHint("{{QUESTIONOPT:q1}}")
            .WithLimitNextAvailableFromDate("{{QUESTIONOPT:q1}}")
            .WithListItems(new List<string> { "{{QUESTIONOPT:q1}}" })
            .Build();

        Page page = new PageBuilder()
            .WithLeadingParagraph("{{QUESTIONOPT:q1}}")
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
                            QuestionId = "q1",
                            Response = "value"
                        }
                    }
                }
            }
        };

        Page result = await _tagParser.Parse(page, formAnswers);

        IElement resultElement = result.Elements.First();

        Assert.Equal("value", result.LeadingParagraph);
        Assert.Equal("value", resultElement.Properties.Text);
        Assert.Equal("value", resultElement.Properties.Hint);
        Assert.Equal("value", resultElement.Properties.LimitNextAvailableFromDate);
        Assert.Equal("value", resultElement.Properties.ListItems.First());
    }

    [Fact]
    public async Task Parse_ShouldCallFormatter_WhenProvided()
    {
        Element element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithPropertyText("{{QUESTIONOPT:q1:testformatter}}")
            .Build();

        Page page = new PageBuilder().WithElement(element).Build();

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
                            QuestionId = "q1",
                            Response = "value"
                        }
                    }
                }
            }
        };

        Page result = await _tagParser.Parse(page, formAnswers);

        Assert.Equal("FAKE-FORMATTED-VALUE", result.Elements.First().Properties.Text);
        _mockFormatter.Verify(x => x.Parse(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Parse_ShouldCall_Multiple_Formatters()
    {
        Element element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithPropertyText("{{QUESTIONOPT:q1:testformatter}} {{QUESTIONOPT:q1:anothertestformatter}}")
            .Build();

        Page page = new PageBuilder().WithElement(element).Build();

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
                            QuestionId = "q1",
                            Response = "value"
                        }
                    }
                }
            }
        };

        Page result = await _tagParser.Parse(page, formAnswers);

        Assert.Equal("FAKE-FORMATTED-VALUE ANOTHER-FORMATTER", result.Elements.First().Properties.Text);
        _mockFormatter.Verify(x => x.Parse(It.IsAny<string>()), Times.Once);
        _mockFormatterTwo.Verify(x => x.Parse(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ParseString_ShouldReturnUpdatedValue()
    {
        string text = "value {{QUESTIONOPT:q1}}";

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
                            QuestionId = "q1",
                            Response = "test"
                        }
                    }
                }
            }
        };

        string result = _tagParser.ParseString(text, formAnswers);

        Assert.Equal("value test", result);
    }

    [Fact]
    public void ParseString_ShouldReturnEmpty_WhenMissing()
    {
        string result = _tagParser.ParseString("value {{QUESTIONOPT:q1}}", new FormAnswers());

        Assert.Equal("value ", result);
    }

    [Fact]
    public void ParseString_ShouldCallFormatter()
    {
        string text = "{{QUESTIONOPT:q1:testformatter}}";

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
                            QuestionId = "q1",
                            Response = "value"
                        }
                    }
                }
            }
        };

        string result = _tagParser.ParseString(text, formAnswers);

        Assert.Equal("FAKE-FORMATTED-VALUE", result);
        _mockFormatter.Verify(x => x.Parse(It.IsAny<string>()), Times.Once);
    }
}