using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models.Elements;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators;

public class EmailElementValidatorTests
{
    private readonly EmailElementValidator _emailElementValidator = new();
    private readonly RequiredElementValidator _requiredElementValidator = new();

    [Fact]
    public void Validate_ShouldShowEmailValidationMessageWhenEmailFieldIsEmpty()
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithQuestionId("email")
            .WithLabel("Enter your email")
            .WithEmail(true)
            .Build();

        Dictionary<string, dynamic> viewModel = new();
        viewModel.Add("email", "notanemail");

        // Act
        ValidationResult result = _emailElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Enter an email address in the correct format, like name@example.com", result.Message);
    }

    [Theory]
    [InlineData("isanemail@stockport.gov.uk")]
    [InlineData("isanemail@stock.port.gov.uk")]
    [InlineData("isanemail@st-ock.port.gov.uk")]
    [InlineData("isanemail@st.ock-port.gov.uk")]
    [InlineData("isanemail@stock.port")]
    public void Validate_ShouldCheckEmailIsValid(string email)
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithQuestionId("email")
            .WithLabel("Enter your email")
            .WithEmail(true)
            .Build();

        Dictionary<string, dynamic> viewModel = new();
        viewModel.Add("email", email);

        // Act
        ValidationResult result = _emailElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldShowNormalValidationMessageWhenEmailFieldIsEmpty()
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithQuestionId("email")
            .WithLabel("Enter your email")
            .WithEmail(true)
            .Build();

        Dictionary<string, dynamic> viewModel = new();

        // Act
        ValidationResult result = _requiredElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Enter the enter your email", result.Message);
    }

    [Fact]
    public void Validate_ShouldShowNoValidationMessageWhenEmailFieldIsEmptyAndOptional()
    {
        // Arrange
        Element element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .WithQuestionId("email")
            .WithLabel("Enter your email")
            .WithEmail(true)
            .WithOptional(true)
            .Build();

        Dictionary<string, dynamic> viewModel = new();
        viewModel.Add("email", "");

        // Act
        ValidationResult result = _emailElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }
}
