using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;

namespace form_builder.Models.Elements;

public class DateInput : Element
{
    public DateInput() => Type = EElementType.DateInput;

    public static string DAY_EXTENSION => "-day";
    public static string MONTH_EXTENSION => "-month";
    public static string YEAR_EXTENSION => "-year";

    public override Task<string> RenderAsync(IViewRender viewRender,
        IElementHelper elementHelper,
        string cacheKey,
        Dictionary<string, dynamic> viewModel,
        Page page,
        FormSchema formSchema,
        IWebHostEnvironment environment,
        FormAnswers formAnswers,
        List<object> results = null)
    {
        Properties.Day = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, DAY_EXTENSION);
        Properties.Month = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, MONTH_EXTENSION);
        Properties.Year = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, YEAR_EXTENSION);
        elementHelper.CheckForQuestionId(this);
        elementHelper.CheckForLabel(this);
        elementHelper.CheckAllDateRestrictionsAreNotEnabled(this);

        return viewRender.RenderAsync(Type.ToString(), this);
    }

    private static bool HasValidDateTimeAnswer(Dictionary<string, dynamic> viewModel, string key)
    {
        var yearKey = $"{key}{DateInput.YEAR_EXTENSION}";
        var monthKey = $"{key}{DateInput.MONTH_EXTENSION}";
        var dayKey = $"{key}{DateInput.DAY_EXTENSION}";

        if (!viewModel.ContainsKey(yearKey) ||
            !viewModel.ContainsKey(monthKey) ||
            !viewModel.ContainsKey(dayKey))
        {
            return false;
        }

        if (string.IsNullOrEmpty(viewModel[yearKey]) ||
            string.IsNullOrEmpty(viewModel[monthKey]) ||
            string.IsNullOrEmpty(viewModel[dayKey]))
        {
            return false;
        }

        return true;
    }

    public static DateTime? GetDate(Dictionary<string, dynamic> viewModel, string key)
    {

        var yearKey = $"{key}{DateInput.YEAR_EXTENSION}";
        var monthKey = $"{key}{DateInput.MONTH_EXTENSION}";
        var dayKey = $"{key}{DateInput.DAY_EXTENSION}";

        if (!HasValidDateTimeAnswer(viewModel, key))
            return null;

        string year = viewModel[yearKey];
        string month = viewModel[monthKey];
        string day = viewModel[dayKey];

        return new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
    }

    public static DateTime? GetDate(FormAnswers answers, string key)
    {
        IEnumerable<Answers> flattenedAnswers = answers.AllAnswers;
        string year = flattenedAnswers.Single(answer => answer.QuestionId.Equals($"{key}{DateInput.YEAR_EXTENSION}")).Response;
        string month = flattenedAnswers.Single(answer => answer.QuestionId.Equals($"{key}{DateInput.MONTH_EXTENSION}")).Response;
        string day = flattenedAnswers.Single(answer => answer.QuestionId.Equals($"{key}{DateInput.DAY_EXTENSION}")).Response;

        return new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
    }
}