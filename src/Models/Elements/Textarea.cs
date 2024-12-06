using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;

namespace form_builder.Models.Elements;

public class Textarea : Element
{
    public bool DisplayCharacterCount => Properties.DisplayCharacterCount;
    public Textarea() => Type = EElementType.Textarea;

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
        elementHelper.CheckForMaxLength(this);

        return viewRender.RenderAsync(Type.ToString(), this);
    }

    public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
    {
        var properties = new Dictionary<string, dynamic>()
        {
            { "name", Properties.QuestionId },
            { "id", Properties.QuestionId },
            { "value", Properties.Value},
            { "spellcheck", Properties.Spellcheck.ToString().ToLower() },
            { "rows", Properties.MaxLength > 500 ? "15" : "5" }
        };

        if (!DisplayAriaDescribedby)
            return properties;

        properties.Add("aria-describedby",
            DisplayCharacterCount
                ? $"{GetCustomItemId("info")} {GetDescribedByAttributeValue()}"
                : GetDescribedByAttributeValue());

        return properties;
    }
}