using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Element;

public class TagParserCheckTests
{
    private readonly TagParserCheck _integrityCheck = new();

    [Theory]
    [InlineData("{{STRONG::text}}")]
    [InlineData("{{BOLD::text}}")]
    [InlineData("{{EMPHASIS::text}}")]
    [InlineData("{{ITALIC::text}}")]
    [InlineData("{{IMAGE::text::alt text}}")]
    public void Validate_ShouldReturn_True_ForText(string tagParserValue)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithPropertyText(tagParserValue)
            .Build();

        // Act & Assert
        var result = _integrityCheck.Validate(element);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("{{STRONG::text}}")]
    [InlineData("{{BOLD::text}}")]
    [InlineData("{{EMPHASIS::text}}")]
    [InlineData("{{ITALIC::text}}")]
    [InlineData("{{IMAGE::text::alt text}}")]
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
    [InlineData("{{STRONG:text}}")]
    [InlineData("{{BOLD:text}}")]
    [InlineData("{{EMPHASIS:text}}")]
    [InlineData("{{ITALIC:text}}")]
    [InlineData("{{IMAGE:text::alt text}}")]
    public void Validate_ShouldReturn_False_ForText(string tagParserValue)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.P)
            .WithPropertyText(tagParserValue)
            .Build();

        // Act & Assert
        var result = _integrityCheck.Validate(element);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("{{STRONG:text}}")]
    [InlineData("{{BOLD:text}}")]
    [InlineData("{{EMPHASIS:text}}")]
    [InlineData("{{ITALIC:text}}")]
    [InlineData("{{IMAGE:text::alt text}}")]
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
}
