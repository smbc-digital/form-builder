namespace form_builder_tests.UnitTests.Validators;

public class TimeInputElementValidatorTests
{
    private readonly TimeInputValidator _dateInputElementValidator = new();

    [Fact]
    public void Validate_ShouldCheckTheElementTypeIsNotDateInput()
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.Textbox)
            .Build();

        // Assert
        var result = _dateInputElementValidator.Validate(element, null, new form_builder.Models.FormSchema());
        Assert.True(result.IsValid);
    }
    [Fact]
    public void Validate_ShouldShowValidationMessageWhenFieldsAreEmpty()
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.TimeInput)
            .WithQuestionId("test-time")
            .WithLabel("Time")
            .Build();

        Dictionary<string, dynamic> viewModel = new();

        // Act
        var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Check the time and try again", result.Message);
    }

    [Fact]
    public void Validate_ShouldCheckTimeIsNotValid()
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.TimeInput)
            .WithQuestionId("test-time")
            .Build();

        var viewModel = new Dictionary<string, dynamic>()
        {
            {$"test-time{TimeConstants.HOURS_SUFFIX}", "aa"},
            {$"test-time{TimeConstants.MINUTES_SUFFIX}s", "aa"},
            {$"test-time{TimeConstants.AM_PM_SUFFIX}", "aaaa"}
        };

        // Act
        var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Check the time and try again", result.Message);
    }

    [Theory]
    [InlineData("6", "AM")]
    [InlineData("7", "AM")]
    [InlineData("6", "PM")]
    [InlineData("7", "PM")]
    public void Validate_ShouldCheckMinTime_IsValid_IfProvided(string hour, string suffix)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.TimeInput)
            .WithQuestionId("test-time")
            .WithMinTime("05:00")
            .Build();

        var viewModel = new Dictionary<string, dynamic>()
        {
            {$"test-time{TimeConstants.HOURS_SUFFIX}", hour},
            {$"test-time{TimeConstants.MINUTES_SUFFIX}", "04"},
            {$"test-time{TimeConstants.AM_PM_SUFFIX}", suffix}
        };

        // Act
        var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("6", "AM")]
    [InlineData("7", "AM")]
    public void Validate_ShouldCheckMinTime_IsNotValid_IfProvided(string hour, string suffix)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.TimeInput)
            .WithQuestionId("test-time")
            .WithMinTime("09:00")
            .Build();

        var viewModel = new Dictionary<string, dynamic>()
        {
            {$"test-time{TimeConstants.HOURS_SUFFIX}", hour},
            {$"test-time{TimeConstants.MINUTES_SUFFIX}", "04"},
            {$"test-time{TimeConstants.AM_PM_SUFFIX}", suffix}
        };

        // Act
        var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Check the time and try again", result.Message);
    }

    [Theory]
    [InlineData("6", "AM")]
    [InlineData("7", "AM")]
    [InlineData("6", "PM")]
    [InlineData("7", "PM")]
    public void Validate_ShouldCheckMaxTime_IsValid_IfProvided(string hour, string suffix)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.TimeInput)
            .WithQuestionId("test-time")
            .WithMaxTime("20:00")
            .Build();

        var viewModel = new Dictionary<string, dynamic>()
        {
            {$"test-time{TimeConstants.HOURS_SUFFIX}", hour},
            {$"test-time{TimeConstants.MINUTES_SUFFIX}", "04"},
            {$"test-time{TimeConstants.AM_PM_SUFFIX}", suffix}
        };

        // Act
        var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("6", "AM")]
    [InlineData("7", "AM")]
    public void Validate_ShouldCheckMaxTime_IsNotValid_IfProvided(string hour, string suffix)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.TimeInput)
            .WithQuestionId("test-time")
            .WithMaxTime("03:00")
            .Build();

        var viewModel = new Dictionary<string, dynamic>()
        {
            {$"test-time{TimeConstants.HOURS_SUFFIX}", hour},
            {$"test-time{TimeConstants.MINUTES_SUFFIX}", "04"},
            {$"test-time{TimeConstants.AM_PM_SUFFIX}", suffix}
        };

        // Act
        var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Check the time and try again", result.Message);
    }

    [Theory]
    [InlineData("6", "AM")]
    [InlineData("7", "AM")]
    [InlineData("6", "PM")]
    [InlineData("7", "PM")]
    public void Validate_ShouldCheckMaxTimeAndMinTime_IsValid_IfProvided(string hour, string suffix)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.TimeInput)
            .WithQuestionId("test-time")
            .WithMinTime("05:00")
            .WithMaxTime("20:00")
            .Build();

        var viewModel = new Dictionary<string, dynamic>()
        {
            {$"test-time{TimeConstants.HOURS_SUFFIX}", hour},
            {$"test-time{TimeConstants.MINUTES_SUFFIX}", "04"},
            {$"test-time{TimeConstants.AM_PM_SUFFIX}", suffix}
        };

        // Act
        var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("6", "AM")]
    [InlineData("7", "AM")]
    [InlineData("6", "PM")]
    [InlineData("7", "PM")]
    public void Validate_ShouldCheckMaxTimeAndMinTime_IsNotValid_IfProvided(string hour, string suffix)
    {
        // Arrange
        var element = new ElementBuilder()
            .WithType(EElementType.TimeInput)
            .WithQuestionId("test-time")
            .WithMinTime("09:00")
            .WithMaxTime("17:00")
            .Build();

        var viewModel = new Dictionary<string, dynamic>()
        {
            {$"test-time{TimeConstants.HOURS_SUFFIX}", hour},
            {$"test-time{TimeConstants.MINUTES_SUFFIX}", "04"},
            {$"test-time{TimeConstants.AM_PM_SUFFIX}", suffix}
        };

        // Act
        var result = _dateInputElementValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Check the time and try again", result.Message);
    }
}