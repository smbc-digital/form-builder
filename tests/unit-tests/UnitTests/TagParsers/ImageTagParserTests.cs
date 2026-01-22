using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers;

public class ImageTagParserTests
{
    private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new();
    private readonly ImageTagParser _tagParser;

    public ImageTagParserTests()
    {
        _tagParser = new ImageTagParser(_mockFormatters.Object);
    }

    [Theory]
    [InlineData("{{IMAGE::https://www.google.com::Alt Text}}")]
    public void Regex_ShouldReturnTrue_Result(string value)
    {
        Assert.True(_tagParser.Regex.Match(value).Success);
    }

    [Theory]
    [InlineData("{{IMAGE:https://www.google.com:Alt Text}}")]
    [InlineData("{{IMG::https://www.google.com::Alt Text}}")]
    [InlineData("{{IMAGE:ref}}")]
    [InlineData("{IMAGE::https://www.google.com::Alt Text}")]
    [InlineData("{{IMAGE::https://www.google.com::Alt Text}")]
    [InlineData("{{TAG:firstname}")]
    public void Regex_ShouldReturnFalse_Result(string value)
    {
        Assert.False(_tagParser.Regex.Match(value).Success);
    }

    [Fact]
    public void FormatContent_ShouldReturnValidFormattedText()
    {
        var url = "https://www.stockport.gov.uk";
        var altText = "alt text";
        var expectValue = string.Format(_tagParser._htmlContent, url, altText);
        Assert.Equal(expectValue, _tagParser.FormatContent(new string[2] { url, altText }));
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
            .WithPropertyText("this value {{TAG:firstname}} should be replaced with name question")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);

        Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
    }

    [Fact]
    public async Task Parse_ShouldReturn_UpdatedText_WithReplacedValue()
    {
        var expectedString = $"this image {_tagParser.FormatContent(new string[2] { "https://www.stockport.gov", "alt text" })} should be replaced";

        var element = new ElementBuilder()
           .WithType(EElementType.P)
           .WithPropertyText("this image {{IMAGE::https://www.stockport.gov::alt text}} should be replaced")
           .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var result = await _tagParser.Parse(page, new FormAnswers());
        Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
    }

    [Fact]
    public async Task Parse_ShouldReturn_UpdatedHint_WithReplacedValue()
    {
        var expectedString = $"this image {_tagParser.FormatContent(new string[2] { "https://www.stockport.gov", "alt text" })} should be replaced";

        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithHint("this image {{IMAGE::https://www.stockport.gov::alt text}} should be replaced")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var result = await _tagParser.Parse(page, new FormAnswers());
        Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Hint);
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
    public void ParseString_ShouldReturn_UpdatedText_WithReplacedValue()
    {
        var expectedString = $"this image {_tagParser.FormatContent(new string[2] { "https://www.stockport.gov", "alt text" })} should be replaced";

        var text = "this image {{IMAGE::https://www.stockport.gov::alt text}} should be replaced";

        var result = _tagParser.ParseString(text, new FormAnswers());
        Assert.Equal(expectedString, result);
    }
}
