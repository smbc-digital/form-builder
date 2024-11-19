using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;

namespace form_builder.Models.Elements;

public class InlineAlert : Element
{
    public InlineAlert() => Type = EElementType.InlineAlert;

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
        elementHelper.CheckIfLabelAndTextEmpty(this);

        return viewRender.RenderAsync(Type.ToString(), this);
    }
}