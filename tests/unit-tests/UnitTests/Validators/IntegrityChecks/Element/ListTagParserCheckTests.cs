using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Element;

public class ListTagParserCheckTests
{
    private readonly ListTagParserCheck _integrityCheck = new();

    [Theory]
    [InlineData("{{ULIST::text}}")]
    [InlineData("{{OLIST::text}}")]
    public void Validate_ShouldReturn_True_ForIag(string tagParserValue)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithIAG(tagParserValue)
            .Build();

        // Act & Assert
        var result = _integrityCheck.Validate(element);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("{{ULIST::text}}")]
    [InlineData("{{OLIST::text}}")]
    public void Validate_ShouldReturn_True_ForHint(string tagParserValue)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithHint(tagParserValue)
            .Build();

        // Act & Assert
        var result = _integrityCheck.Validate(element);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(EElementType.UL, "{{ULIST::text}}")]
    [InlineData(EElementType.OL, "{{OLIST::text}}")]
    public void Validate_ShouldReturn_True_ForListItems(EElementType type, string tagParserValue)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(type)
            .WithListItems([tagParserValue])
            .Build();

        // Act & Assert
        var result = _integrityCheck.Validate(element);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("{{ULIST:text}}")]
    [InlineData("{{OLIST:text}}")]
    public void Validate_ShouldReturn_False_ForIag(string tagParserValue)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithIAG(tagParserValue)
            .Build();

        // Act & Assert
        var result = _integrityCheck.Validate(element);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("{{ULIST:text}}")]
    [InlineData("{{OLIST:text}}")]
    public void Validate_ShouldReturn_False_ForHint(string tagParserValue)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithHint(tagParserValue)
            .Build();

        // Act & Assert
        var result = _integrityCheck.Validate(element);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(EElementType.UL, "{{ULIST:text}}")]
    [InlineData(EElementType.OL, "{{OLIST:text}}")]
    public void Validate_ShouldReturn_False_ForListItems(EElementType type, string tagParserValue)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(type)
            .WithListItems([tagParserValue])
            .Build();

        // Act & Assert
        var result = _integrityCheck.Validate(element);
        Assert.False(result.IsValid);
    }
}
