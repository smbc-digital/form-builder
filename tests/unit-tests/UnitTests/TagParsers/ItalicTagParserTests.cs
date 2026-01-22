using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers;

public class ItalicTagParserTests
{
    private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new();
    private readonly ItalicTagParser _tagParser;

    public ItalicTagParserTests()
    {
        _tagParser = new ItalicTagParser(_mockFormatters.Object);
    }

    [Theory]
    [InlineData("{{ITALIC::text}}")]
    [InlineData("{{ITALIC::text of multiple words}}")]
    public void Regex_ShouldReturnTrue_Result(string value)
    {
        Assert.True(_tagParser.Regex.Match(value).Success);
    }

    [Theory]
    [InlineData("{{ITALOC::text}}")]
    [InlineData("{{ITALICC::text}}")]
    [InlineData("{{italic::text}}")]
    [InlineData("{ITALIC::text}")]
    [InlineData("{{ITALIC:text}}")]
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
        var expectedString = "this <i>text</i> should have italic tags";

        var element = new ElementBuilder()
           .WithType(EElementType.P)
           .WithPropertyText("this {{ITALIC::text}} should have italic tags")
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
        var expectedString = "this <i>text</i> should have italic tags";

        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithHint("this {{ITALIC::text}} should have italic tags")
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
        var expectedString = "this <i>text</i> should have italic tags, and so should <i>this text</i>";

        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithPropertyText("this {{ITALIC::text}} should have italic tags, and so should {{ITALIC::this text}}")
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
        var expectedString = "this <i>text</i> should have italic tags";
        var text = "this {{ITALIC::text}} should have italic tags";

        var formAnswers = new FormAnswers();

        var result = _tagParser.ParseString(text, formAnswers);

        Assert.Equal(expectedString, result);
    }

    [Fact]
    public void ParseString_ShouldReturnUpdatedValue_WhenReplacingMultipleValues()
    {
        var expectedString = "this <i>text</i> should have italic tags, and so should <i>this text</i>";
        var text = "this {{ITALIC::text}} should have italic tags, and so should {{ITALIC::this text}}";

        var formAnswers = new FormAnswers();

        var result = _tagParser.ParseString(text, formAnswers);

        Assert.Equal(expectedString, result);
    }
}
