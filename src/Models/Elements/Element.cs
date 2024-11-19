using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Validators;

namespace form_builder.Models.Elements;

public class Element : IElement
{
    protected ValidationResult validationResult;

    public Element() => validationResult = new ValidationResult();

    public EElementType Type { get; set; }

    public BaseProperty Properties { get; set; }

    public virtual bool DisplayHint => !string.IsNullOrEmpty(Properties.Hint.Trim());

    public bool HadCustomClasses => !string.IsNullOrEmpty(Properties.ClassName);

    public virtual string QuestionId => Properties.QuestionId;

    public virtual string Label => Properties.Label;

    public virtual string Hint => Properties.Hint;

    public virtual string HintId => $"{QuestionId}-hint";

    public virtual string ErrorId => $"{QuestionId}-error";

    public virtual string Warning => Properties.Warning;

    public virtual string WarningID => $"{QuestionId}-warning";

    public bool DisplayAriaDescribedby => DisplayHint || !IsValid;

    public bool IsValid => validationResult.IsValid;

    public bool IsModelStateValid { get; set; } = true;

    public string ValidationMessage => validationResult.Message;

    public string Lookup { get; set; }

    public void Validate(Dictionary<string, dynamic> viewModel, IEnumerable<IElementValidator> validators, FormSchema baseForm)
    {
        foreach (var validator in validators)
        {
            var result = validator.Validate(this, viewModel, baseForm);
            if (!result.IsValid)
            {
                validationResult = result;
                return;
            }
        }
    }

    public virtual string GenerateFieldsetProperties() => string.Empty;

    public virtual Dictionary<string, dynamic> GenerateElementProperties(string type = "") => new();

    public string GetListItemId(int index) => $"{QuestionId}-{index}";

    public string GetCustomItemId(string key) => $"{QuestionId}-{key}";

    public string GetCustomHintId(string key) => $"{GetCustomItemId(key)}-hint";

    public string GetCustomErrorId(string key) => $"{GetCustomItemId(key)}-error";

    public string GetListItemHintId(int index) => $"{GetListItemId(index)}-hint";

    public string DescribedByAttribute() => DisplayAriaDescribedby ? $"aria-describedby=\"{GetDescribedByAttributeValue()}\"" : string.Empty;

    public string GetDescribedByAttributeValue() => CreateDescribedByAttributeValue($"{QuestionId}");

    public string GetDescribedByAttributeValue(string prefix) => CreateDescribedByAttributeValue($"{QuestionId}{prefix}");

    private string CreateDescribedByAttributeValue(string key)
    {
        var describedBy = new List<string>();
        if (DisplayHint)
            describedBy.Add(HintId);

        if (!IsValid)
            describedBy.Add(ErrorId);

        return string.Join(" ", describedBy);
    }

    public string WriteHtmlForAndClassAttribute(string prefix = "")
    {
        var data = string.Empty;

        if (DisplayOptional)
            data = "class = smbc-body";

        return !Properties.LegendAsH1 ? $"{data} for = {QuestionId}{prefix}" : data;
    }

    public string WriteOptional(string prefix = "") => DisplayOptional ? "class = optional" : null;

    public virtual Task<string> RenderAsync(IViewRender viewRender,
        IElementHelper elementHelper,
        string cacheKey,
        Dictionary<string, dynamic> viewModel,
        Page page,
        FormSchema formSchema,
        IWebHostEnvironment environment,
        FormAnswers formAnswers,
        List<object> results = null) => viewRender.RenderAsync(Type.ToString(), this, null);

    private bool DisplayOptional => Properties.Optional;

    public virtual string GetLabelText(string pageTitle) => $"{(string.IsNullOrEmpty(Properties.SummaryLabel) ? Properties.Label : Properties.SummaryLabel)}{GetIsOptionalLabelText()}";

    protected string GetIsOptionalLabelText() => $"{(Properties.Optional ? " (optional)" : string.Empty)}";
}