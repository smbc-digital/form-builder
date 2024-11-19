using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;

namespace form_builder.Models.Elements;

public class Checkbox : Element
{
    public Checkbox() => Type = EElementType.Checkbox;

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
        elementHelper.CheckForCheckBoxListValues(this);
        elementHelper.OrderOptionsAlphabetically(this);

        return viewRender.RenderAsync(Type.ToString(), this);
    }

    public Dictionary<string, dynamic> GenerateCheckboxProperties(int i)
    {
        var properties = new Dictionary<string, dynamic>
        {
            { "class", "govuk-checkboxes__input" },
            { "type", "checkbox" },
            { "id", GetListItemId(i) },
            { "value", Properties.Options[i].Value},
        };

        if (Properties.Autofocus)
            properties.Add("autofocus", true);

        if (Properties.Options.Any(_ => _.HasConditionalElement))
        {
            var fieldsetIncrement = Properties.QuestionId.Split(':').Length > 1 ? $"-{Properties.QuestionId.Split(':')[1]}" : null;
            properties.Add("data-aria-controls", $"conditional-{i}{fieldsetIncrement}-{Properties.QuestionId}");
        }

        if (Properties.Options[i].HasHint)
            properties.Add("aria-describedby", GetListItemHintId(i));

        if (Properties.Options[i].Exclusive)
            properties.Add("data-behaviour", "exclusive");

        if (Properties.Checked || Properties.Value.Contains(Properties.Options[i].Value))
            properties.Add("checked", "true");

        return properties;
    }
}