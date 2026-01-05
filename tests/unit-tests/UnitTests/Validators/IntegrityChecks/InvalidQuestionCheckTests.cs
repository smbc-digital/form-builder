using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Elements;
using form_builder.Validators.IntegrityChecks.Elements;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks;

public class InvalidQuestionCheckTests
{
    private readonly Mock<IElementHelper> _mockElementHelper = new();

    private readonly InvalidQuestionCheck _check;

    public InvalidQuestionCheckTests()
    {
        _mockElementHelper
            .Setup(mock => mock.IsElementANonInputType(It.IsAny<IElement>()))
            .Returns(false);

        _check = new InvalidQuestionCheck(_mockElementHelper.Object);
    }

    [Fact]
    public void Validate_ShouldReturnTrue_WhenElementIsNotAnInputType()
    {
        // Arrange
        _mockElementHelper
            .Setup(mock => mock.IsElementANonInputType(It.IsAny<IElement>()))
            .Returns(true);

        var element = new ElementBuilder()
            .WithType(EElementType.H1)
            .Build();

        // Act
        var result = _check.Validate(element);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
    }

    [Fact]
    public void Validate_ShouldReturnFalse_WhenPropertiesIsNull()
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.Textarea)
            .Build();

        // Act
        var result = _check.Validate(element);

        // Assert
        Assert.False(result.IsValid);
        var hasFailureMessage = result.Messages.Any(message => message.Contains(IntegrityChecksConstants.FAILURE));
        Assert.True(hasFailureMessage);
    }

    [Fact]
    public void Validate_ShouldReturnTrue_WhenQuestionIdValid()
    {
        // Arrange
        var element = new ElementBuilder()
            .WithQuestionId("validQuestionId")
            .WithType(EElementType.Textarea)
            .Build();

        // Act
        var result = _check.Validate(element);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
    }

    [Theory]
    [InlineData("invalid-questionId")]
    [InlineData("question4")]
    [InlineData("question£")]
    [InlineData("que!stion")]
    [InlineData("quest%ion")]
    [InlineData("question.")]
    [InlineData(".question")]
    public void Validate_ShouldReturnFalse_WhenQuestionIdNotValid(string questionId)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithQuestionId(questionId)
            .WithType(EElementType.Textarea)
            .Build();

        // Act
        var result = _check.Validate(element);

        // Assert
        Assert.False(result.IsValid);
        var hasFailureMessage = result.Messages.Any(message => message.Contains(IntegrityChecksConstants.FAILURE));
        Assert.True(hasFailureMessage);
    }
}