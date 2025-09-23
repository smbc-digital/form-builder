using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators;

public class MinLengthValidatorTests
{
    readonly MinLengthValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnFalse_IfMinLengthNotMet()
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithQuestionId("testId")
            .WithLabel("Label")
            .WithMinLength(5)
            .Build();

        Dictionary<string, dynamic> viewModel = new() { { "testId", "a" } };

        // Act
        ValidationResult result = _validator.Validate(element, viewModel, new FormSchema());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Enter 5 or more characters", result.Message);
    }

    [Fact]
    public void Validate_ShouldReturnTrue_IfMinLengthMet()
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithQuestionId("testId")
            .WithLabel("Label")
            .WithMinLength(5)
            .Build();

        Dictionary<string, dynamic> viewModel = new() { { "testId", "abcde" } };

        // Act
        ValidationResult result = _validator.Validate(element, viewModel, new FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnTrue_IfNumericTextbox()
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithQuestionId("testId")
            .WithLabel("Label")
            .WithNumeric(true)
            .WithMinLength(5)
            .Build();

        Dictionary<string, dynamic> viewModel = new() { { "testId", "1" } };

        // Act
        ValidationResult result = _validator.Validate(element, viewModel, new FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnTrue_IfDecimalTextbox()
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithQuestionId("testId")
            .WithLabel("Label")
            .WithDecimal(true)
            .WithMinLength(5)
            .Build();

        Dictionary<string, dynamic> viewModel = new() { { "testId", "1" } };

        // Act
        ValidationResult result = _validator.Validate(element, viewModel, new FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnTrue_IfMinLengthNull()
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithQuestionId("testId")
            .WithLabel("Label")
            .Build();

        Dictionary<string, dynamic> viewModel = new() { { "testId", "1" } };

        // Act
        ValidationResult result = _validator.Validate(element, viewModel, new FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnTrue_IfOptional()
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithQuestionId("testId")
            .WithLabel("Label")
            .WithMinLength(5)
            .WithOptional(true)
            .Build();

        Dictionary<string, dynamic> viewModel = new() { { "testId", "1" } };

        // Act
        ValidationResult result = _validator.Validate(element, viewModel, new FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReturnTrue_IfNotTextbox()
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textarea)
            .WithQuestionId("testId")
            .WithLabel("Label")
            .WithMinLength(5)
            .Build();

        Dictionary<string, dynamic> viewModel = new() { { "testId", "1" } };

        // Act
        ValidationResult result = _validator.Validate(element, viewModel, new FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }
}
