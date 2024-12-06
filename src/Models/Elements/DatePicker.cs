using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;

namespace form_builder.Models.Elements;

public class DatePicker : Element
{
    public const string PLACEHOLDER_DATE_FORMAT = "dd/mm/yyyy";

    public DatePicker() => Type = EElementType.DatePicker;

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
        Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers);

        elementHelper.CheckForQuestionId(this);
        elementHelper.CheckForLabel(this);
        elementHelper.CheckAllDateRestrictionsAreNotEnabled(this);

        return viewRender.RenderAsync(Type.ToString(), this);
    }

    public override Dictionary<string, dynamic> GenerateElementProperties(string type)
    {
        var todaysDate = DateTime.Now;
        var maxDate = Properties.RestrictFutureDate
            ? Properties.RestrictCurrentDate ? DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd")
            : string.IsNullOrEmpty(Properties.Max) ? todaysDate.AddYears(100).ToString("yyyy-MM-dd") : new DateTime(int.Parse(Properties.Max), todaysDate.Month, todaysDate.Day).ToString("yyyy-MM-dd");

        var minDate = Properties.RestrictPastDate
            ? Properties.RestrictCurrentDate ? DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd")
            : string.Empty;

        var properties = new Dictionary<string, dynamic>()
        {
            { "type", "date" },
            { "id", Properties.QuestionId },
            { "name", Properties.QuestionId },
            { "max", maxDate },
            { "min", minDate },
            { "placeholder", PLACEHOLDER_DATE_FORMAT }
        };

        if (DisplayAriaDescribedby)
        {
            properties.Add("aria-describedby", GetDescribedByAttributeValue());
        }

        return properties;
    }

    public static DateTime? GetDate(Dictionary<string, dynamic> viewModel, string key)
    {
        if (!viewModel.ContainsKey(key) || string.IsNullOrEmpty(viewModel[key]))
            return null;

        if (!DateTime.TryParse(viewModel[key], out DateTime dateValue))
            throw new FormatException("DatePicker.GetDate: The date format was incorrect");

        return dateValue;
    }

    public static DateTime? GetDate(FormAnswers answers, string key)
    {
        IEnumerable<Answers> response = answers.AllAnswers;
        Answers answer = response.SingleOrDefault(_ => _.QuestionId.Equals(key));

        if (answer is null || string.IsNullOrEmpty(answer.Response))
            return null;

        if (!DateTime.TryParse(answer.Response, out DateTime dateValue))
            throw new FormatException("DatePicker.GetDate: The date format was incorrect");

        return dateValue;
    }
}