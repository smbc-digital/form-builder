using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers;

public class ListTagParserTests
{
    private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new();
    private readonly ListTagParser _tagParser;

    public ListTagParserTests()
    {
        _tagParser = new ListTagParser(_mockFormatters.Object);
    }

    [Theory]
    [InlineData("{{ULIST::firstname}}")]
    [InlineData("{{ULIST::firstname|lastname}}")]
    [InlineData("{{OLIST::firstname}}")]
    [InlineData("{{OLIST::firstname|lastname}}")]
    public void Regex_ShouldReturnTrue_Result(string value)
    {
        Assert.True(_tagParser.Regex.Match(value).Success);
    }

    [Theory]
    [InlineData("{{UL:firstname}}")]
    [InlineData("{{OL:firstname}}")]
    [InlineData("{{LIST:ref}}")]
    [InlineData("{ULIST:firstname}")]
    [InlineData("{OLIST:firstname}")]
    [InlineData("{{ULIST:firstname}")]
    [InlineData("{{OLIST:firstname}")]
    [InlineData("{{TAG:firstname}}")]
    public void Regex_ShouldReturnFalse_Result(string value)
    {
        Assert.False(_tagParser.Regex.Match(value).Success);
    }

    [Fact]
    public async Task Parse_ShouldReturnInitialValue_WhenNoValuesAre_To_BeReplaced()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithHint("this has no values to be replaced")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);

        Assert.Equal(element.Properties.Hint, result.Elements.FirstOrDefault().Properties.Hint);
    }

    [Fact]
    public async Task Parse_ShouldReturnInitialValue_When_NoTag_MatchesRegex()
    {
        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithHint("this value {{TAG::firstname}} should not be replaced as it is invalid")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);

        Assert.Equal(element.Properties.Hint, result.Elements.FirstOrDefault().Properties.Hint);
    }

    [Theory]
    [InlineData("ULIST", "ul", "bullet")]
    [InlineData("OLIST", "ol", "number")]
    public async Task Parse_ShouldReturnUpdatedValueForIAG_WhenReplacingSingleValue(string type, string tagValue, string className)
    {
        var expectedString = $"this <{tagValue} class='govuk-list govuk-list--{className}'><li>text</li><li>other text</li></{tagValue}> should have {tagValue} and li tags";

        var element = new ElementBuilder()
           .WithType(EElementType.Textbox)
           .WithIAG(type.Equals("ULIST") ? "this {{ULIST::text|other text}} should have ul and li tags" : "this {{OLIST::text|other text}} should have ol and li tags")
           .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);
        Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.IAG);
    }

    [Theory]
    [InlineData("ULIST", "ul")]
    [InlineData("OLIST", "ol")]
    public async Task Parse_ShouldReturnUpdatedValueForHint_WhenReplacingSingleValue(string type, string tagValue)
    {
        var expectedString = $"this <{tagValue}><li>text</li><li>other text</li></{tagValue}> should have {tagValue} and li tags";

        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithHint(type.Equals("ULIST") ? "this {{ULIST::text|other text}} should have ul and li tags" : "this {{OLIST::text|other text}} should have ol and li tags")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);
        Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Hint);
    }

    [Fact]
    public async Task Parse_ShouldReturnUpdatedValueForListItem_WhenReplacingSingleValue()
    {
        var expectedString = "<ul><li>text</li><li>other text</li></ul>";

        var element = new ElementBuilder()
            .WithType(EElementType.UL)
            .WithListItems(["{{ULIST::text|other text}}"])
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);
        Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.ListItems.First());
    }

    [Fact]
    public async Task Parse_ShouldReturnUpdatedValue_WhenReplacingMultipleValues()
    {
        var expectedString = "<ul><li>text</li></ul><ol><li>text</li></ol>";

        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithHint("{{ULIST::text}}{{OLIST::text}}")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var formAnswers = new FormAnswers();

        var result = await _tagParser.Parse(page, formAnswers);
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
        var text = "this value {{TAG::firstname}} should not be replaced as it is invalid";

        var formAnswers = new FormAnswers();

        var result = _tagParser.ParseString(text, formAnswers);

        Assert.Equal(text, result);
    }

    [Theory]
    [InlineData("ULIST", "ul", "bullet")]
    [InlineData("OLIST", "ol", "number")]
    public void ParseString_ShouldReturnUpdatedValue_WhenReplacingSingleValue(string type, string tagValue, string className)
    {
        var expectedString = $"this <{tagValue} class='govuk-list govuk-list--{className}'><li>text</li><li>other text</li></{tagValue}> should have {tagValue} and li tags";
        var text = type.Equals("ULIST")
            ? "this {{ULIST::text|other text}} should have ul and li tags"
            : "this {{OLIST::text|other text}} should have ol and li tags";

        var formAnswers = new FormAnswers();

        var result = _tagParser.ParseString(text, formAnswers);

        Assert.Equal(expectedString, result);
    }

    [Fact]
    public void ParseString_ShouldReturnUpdatedValue_WhenReplacingMultipleValues()
    {
        var expectedString = "<ul class='govuk-list govuk-list--bullet'><li>text</li></ul><ol class='govuk-list govuk-list--number'><li>text</li></ol>";
        var text = "{{ULIST::text}}{{OLIST::text}}";

        var formAnswers = new FormAnswers();

        var result = _tagParser.ParseString(text, formAnswers);

        Assert.Equal(expectedString, result);
    }
}
