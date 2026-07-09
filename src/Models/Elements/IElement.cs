using Newtonsoft.Json.Converters;

namespace form_builder.Models.Elements;

[JsonConverter(typeof(JsonSubtypes), "Type")]
public interface IElement
{
    [JsonConverter(typeof(StringEnumConverter))]
    EElementType Type { get; set; }

    BaseProperty Properties { get; set; }

    string Lookup { get; set; }

    bool IsValid { get; }

    void Validate(Dictionary<string, dynamic> viewModel, IEnumerable<IElementValidator> validators, FormSchema baseForm);

    Task<string> RenderAsync(IViewRender viewRender,
        IElementHelper elementHelper,
        string cacheKey,
        Dictionary<string, dynamic> viewModel,
        Page page,
        FormSchema formSchema,
        IWebHostEnvironment environment,
        FormAnswers formAnswers,
        List<object> results = null);

    Dictionary<string, dynamic> GenerateElementProperties(string type = "");

    string GenerateFieldsetProperties();

    string GetLabelText(string pageTitle);
}