using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;

namespace form_builder.Models.Elements;

public class Select : Element
{
    public Select() => Type = EElementType.Select;

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
        elementHelper.ReSelectPreviousSelectedOptions(this);
        elementHelper.CheckForLabel(this);
        elementHelper.CheckForSelectOptions(this);

        return viewRender.RenderAsync(Type.ToString(), this);
    }
}