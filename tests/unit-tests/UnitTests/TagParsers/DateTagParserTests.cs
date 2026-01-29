using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers.Formatters;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers;

public class DateTagParserTests
{
    private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new();
    private readonly DateTagParser _tagParser;

    public DateTagParserTests()
    {
        _tagParser = new DateTagParser(_mockFormatters.Object);
    }

    [Theory]
    [InlineData("{{DATE:+5y}}")]
    [InlineData("{{DATE:-2m}}")]
    [InlineData("{{DATE:-4w}}")]
    [InlineData("{{DATE:+10d}}")]
    [InlineData("{{DATE:nextweek}}")]
    [InlineData("{{DATE:childbirthyear}}")]
    public void Regex_ShouldReturnTrue_Result(string value)
    {
        Assert.True(_tagParser.Regex.Match(value).Success);
    }

    [Theory]
    [InlineData("{{date:+5y}}")]
    [InlineData("{{dAtE:+5y}}")]
    [InlineData("{DATE:+5y}")]
    [InlineData("{{DATE+5y}}")]
    [InlineData("{{TAG:+5y}}")]
    public void Regex_ShouldReturnFalse_Result(string value)
    {
        Assert.False(_tagParser.Regex.Match(value).Success);
    }

    [Fact]
    public async Task Parse_ShouldReturnInitialValue_WhenNoValuesAre_To_BeReplaced()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithHint("no tag")
            .Build();

        var page = new PageBuilder().WithElement(element).Build();
        var result = await _tagParser.Parse(page, new FormAnswers());

        Assert.Equal("no tag", result.Elements.First().Properties.Hint);
    }

    [Fact]
    public async Task Parse_ShouldReturnInitialValue_When_NoTag_MatchesRegex()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithHint("this {{TAG:nextweek}} should not change")
            .Build();

        var page = new PageBuilder().WithElement(element).Build();
        var result = await _tagParser.Parse(page, new FormAnswers());

        Assert.Equal("this {{TAG:nextweek}} should not change", result.Elements.First().Properties.Hint);
    }

    [Fact]
    public async Task Parse_ShouldReplace_Year()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithHint("One year from now should be {{DATE:+1y}}")
            .Build();

        var page = new PageBuilder().WithElement(element).Build();
        var result = await _tagParser.Parse(page, new FormAnswers());

        var expected = DateTime.Now.AddYears(1).ToString("dd MM yyyy");
        Assert.Equal($"One year from now should be {expected}", result.Elements.First().Properties.Hint);
    }

    [Fact]
    public async Task Parse_ShouldReplace_Month()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithHint("Two months from now should be {{DATE:+2m}}")
            .Build();

        var page = new PageBuilder().WithElement(element).Build();
        var result = await _tagParser.Parse(page, new FormAnswers());

        var expected = DateTime.Now.AddMonths(2).ToString("dd MM yyyy");
        Assert.Equal($"Two months from now should be {expected}", result.Elements.First().Properties.Hint);
    }

    [Fact]
    public async Task Parse_ShouldReplace_Day()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithHint("Ten days from now should be {{DATE:+10d}}")
            .Build();

        var page = new PageBuilder().WithElement(element).Build();
        var result = await _tagParser.Parse(page, new FormAnswers());

        var expected = DateTime.Now.AddDays(10).ToString("dd MM yyyy");
        Assert.Equal($"Ten days from now should be {expected}", result.Elements.First().Properties.Hint);
    }

    [Fact]
    public async Task Parse_ShouldReplace_NamedKeyword()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithHint("Next week should be {{DATE:nextWeek}}")
            .Build();

        var page = new PageBuilder().WithElement(element).Build();
        var result = await _tagParser.Parse(page, new FormAnswers());

        var expected = DateTime.Now.AddDays(7).ToString("dd MM yyyy");
        Assert.Equal($"Next week should be {expected}", result.Elements.First().Properties.Hint);
    }

    [Fact]
    public async Task Parse_ShouldReplaceMultipleValues()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithHint("Start {{DATE:+1d}} end {{DATE:+2d}}")
            .Build();

        var page = new PageBuilder().WithElement(element).Build();
        var result = await _tagParser.Parse(page, new FormAnswers());

        var expected1 = DateTime.Now.AddDays(1).ToString("dd MM yyyy");
        var expected2 = DateTime.Now.AddDays(2).ToString("dd MM yyyy");

        Assert.Equal($"Start {expected1} end {expected2}", result.Elements.First().Properties.Hint);
    }

    [Fact]
    public void ParseString_ShouldReturnInitialValue_WhenNoValuesAre_To_BeReplaced()
    {
        var text = "this has no date tags";
        var result = _tagParser.ParseString(text, new FormAnswers());
        Assert.Equal(text, result);
    }

    [Fact]
    public void ParseString_ShouldReturnInitialValue_When_NoTag_MatchesRegex()
    {
        var text = "this {{TAG:nextweek}} should not change";
        var result = _tagParser.ParseString(text, new FormAnswers());
        Assert.Equal(text, result);
    }

    [Fact]
    public void ParseString_ShouldReplaceMultipleValues()
    {
        var text = "Start {{DATE:+1d}} end {{DATE:+2d}}";

        var expected1 = DateTime.Now.AddDays(1).ToString("dd MM yyyy");
        var expected2 = DateTime.Now.AddDays(2).ToString("dd MM yyyy");

        var result = _tagParser.ParseString(text, new FormAnswers());
        Assert.Equal($"Start {expected1} end {expected2}", result);
    }
}
