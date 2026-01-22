using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers;

public class BreakTagParserTests
{
    private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new();
    private readonly BreakTagParser _tagParser;

    public BreakTagParserTests()
    {
        _tagParser = new BreakTagParser(_mockFormatters.Object);
    }

    [Theory]
    [InlineData("{{BREAK}}")]
    public void Regex_ShouldReturnTrue_Result(string value)
    {
        Assert.True(_tagParser.Regex.Match(value).Success);
    }

    [Theory]
    [InlineData("{{BRAEK}}")]
    [InlineData("{{BREAKK}}")]
    [InlineData("{{break}}")]
    [InlineData("{BREAK}")]
    [InlineData("{{BREAK:text}}")]
    [InlineData("{{BREAK::text}}")]
    [InlineData("{{TAG::text}}")]
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

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);

        Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
    }

    [Fact]
    public async Task Parse_ShouldReturnInitialValue_When_NoTag_MatchesRegex()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithPropertyText("this value {{TAG::firstname}} should not be replaced as it is invalid")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);

        Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
    }

    [Fact]
    public async Task Parse_ShouldReturnUpdatedValue_WhenReplacingSingleValue()
    {
        var expectedString = "this <br> should exist";

        var element = new ElementBuilder()
           .WithType(EElementType.P)
           .WithPropertyText("this {{BREAK}} should exist")
           .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);
        Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
    }

    [Fact]
    public async Task Parse_ShouldReturnUpdatedValueForHint_WhenReplacingSingleValue()
    {
        var expectedString = "this <br> should exist";

        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithHint("this {{BREAK}} should exist")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);
        Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Hint);
    }

    [Fact]
    public async Task Parse_ShouldReturnUpdatedValue_WhenReplacingMultipleValues()
    {
        var expectedString = "this <br> should exist, and so should <br>";

        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithPropertyText("this {{BREAK}} should exist, and so should {{BREAK}}")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);
        Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
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
        var text = "this value {{TAG::firstname}} should not be replaced as it is invalid";

        var formAnswers = new FormAnswers();

        var result = _tagParser.ParseString(text, formAnswers);

        Assert.Equal(text, result);
    }

    [Fact]
    public void ParseString_ShouldReturnUpdatedValue_WhenReplacingSingleValue()
    {
        var expectedString = "this <br> should exist";
        var text = "this {{BREAK}} should exist";

        var formAnswers = new FormAnswers();

        var result = _tagParser.ParseString(text, formAnswers);

        Assert.Equal(expectedString, result);
    }

    [Fact]
    public void ParseString_ShouldReturnUpdatedValue_WhenReplacingMultipleValues()
    {
        var expectedString = "this <br> should exist, and so should <br>";
        var text = "this {{BREAK}} should exist, and so should {{BREAK}}";

        var formAnswers = new FormAnswers();

        var result = _tagParser.ParseString(text, formAnswers);

        Assert.Equal(expectedString, result);
    }
}
