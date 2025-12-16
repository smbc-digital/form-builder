using form_builder_tests.Builders;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form;

public class AnswerLookupQuestionIdExistsCheckTests
{
    [Fact]
    public void CheckForAnswerLookupQuestionIdExists_ShouldReturnTrue_IfNoElementsUseAnswerLookup()
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.Checkbox)
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var schema = new FormSchemaBuilder()
            .WithName("test-name")
            .WithPage(page)
            .Build();

        AnswerLookupQuestionIdExistsCheck check = new();

        // Act
        var result = check.Validate(schema);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CheckForAnswerLookupQuestionIdExists_ShouldReturnTrue_IfElementsUseNonAnswerLookup()
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.Checkbox)
            .WithLookup("lookup")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .Build();

        var schema = new FormSchemaBuilder()
            .WithName("test-name")
            .WithPage(page)
            .Build();

        AnswerLookupQuestionIdExistsCheck check = new();

        // Act
        var result = check.Validate(schema);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CheckForAnswerLookupQuestionIdExists_ShouldReturnTrue_IfElementUsesAnswerLookup_AndLookupQuestionIdExists()
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.Checkbox)
            .WithLookup("#lookup")
            .Build();

        var pageAction = new ActionBuilder()
            .WithActionType(EActionType.RetrieveExternalData)
            .WithTargetQuestionId("lookup")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .WithPageActions(pageAction)
            .Build();

        var schema = new FormSchemaBuilder()
            .WithName("test-name")
            .WithPage(page)
            .Build();

        AnswerLookupQuestionIdExistsCheck check = new();

        // Act
        var result = check.Validate(schema);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CheckForAnswerLookupQuestionIdExists_ShouldReturnFalse_IfElementUsesAnswerLookup_AndLookupQuestionIdDoesNotExist()
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.Checkbox)
            .WithLookup("#lookup")
            .Build();

        var pageAction = new ActionBuilder()
            .WithActionType(EActionType.RetrieveExternalData)
            .WithTargetQuestionId("action")
            .Build();

        var page = new PageBuilder()
            .WithElement(element)
            .WithPageActions(pageAction)
            .Build();

        var schema = new FormSchemaBuilder()
            .WithName("test-name")
            .WithPage(page)
            .Build();

        AnswerLookupQuestionIdExistsCheck check = new();

        // Act
        var result = check.Validate(schema);

        // Assert
        Assert.False(result.IsValid);
    }
}
