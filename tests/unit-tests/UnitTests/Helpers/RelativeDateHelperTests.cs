namespace form_builder_tests.UnitTests.Helpers;

public class RelativeDateHelperTests
{
    IRelativeDateHelper _relativeDateHelper = new RelativeDateHelper();

    [Fact]
    public void HasValidDate_ShouldReturnTrue_IfDateIsValid()
    {
        // Arange
        var element = new ElementBuilder()
            .WithType(EElementType.DateInput)
            .WithQuestionId("test-date")
            .WithIsFutureDateAfterRelative("3-m-in", "Date is too soon")
            .Build();

        var viewModel = new Dictionary<string, dynamic>
        {
            {"test-date-day", 1},
            {"test-date-month", 1},
            {"test-date-year", 2020}
        };

        // Act
        var result = _relativeDateHelper.HasValidDate(element, viewModel);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasValidDate_ShouldReturnFalse_IfViewModelIsInvalid()
    {
        // Arange
        var element = new ElementBuilder()
            .WithType(EElementType.DateInput)
            .WithQuestionId("test-date")
            .WithIsFutureDateAfterRelative("3-m-in", "Date is too soon")
            .Build();

        var viewModel = new Dictionary<string, dynamic>
        {
            {"badDayString", 1},
            {"badMonthString", 1},
            {"badYearString", 2020}
        };

        // Act
        var result = _relativeDateHelper.HasValidDate(element, viewModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasValidDate_ShouldReturnFalse_IfDateValuesAreNull()
    {
        // Arange
        var element = new ElementBuilder()
            .WithType(EElementType.DateInput)
            .WithQuestionId("test-date")
            .WithIsFutureDateAfterRelative("3-m-in", "Date is too soon")
            .Build();

        var viewModel = new Dictionary<string, dynamic>
        {
            {"test-date-day", null},
            {"test-date-month", null},
            {"test-date-year", null}
        };

        // Act
        var result = _relativeDateHelper.HasValidDate(element, viewModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ChosenDate_ShouldCorrectlyParseValues_IntoDateTimeObject()
    {
        // Arange
        var element = new ElementBuilder()
            .WithType(EElementType.DateInput)
            .WithQuestionId("test-date")
            .WithIsFutureDateAfterRelative("3-m-in", "Date is too soon")
            .Build();

        var viewModel = new Dictionary<string, dynamic>
        {
            {"test-date-day", 1},
            {"test-date-month", 1},
            {"test-date-year", 2020}
        };

        // Act
        var result = _relativeDateHelper.GetChosenDate(element, viewModel);

        // Assert
        Assert.Equal(new DateTime(2020, 1, 1), result);
    }

    [Fact]
    public void GetRelativeDate_ShouldCorrectlyParseValues_IntoRelativeDateObject()
    {
        // Arange
        var relativeDateString = "3-y-ex";

        // Act
        var result = _relativeDateHelper.GetRelativeDate(relativeDateString);

        // Assert
        Assert.Equal(3, result.Amount);
        Assert.Equal(DateInputConstants.YEAR, result.Unit);
        Assert.Equal(DateInputConstants.EXCLUSIVE, result.Type);
    }
}
