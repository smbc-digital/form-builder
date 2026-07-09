namespace form_builder.Models.Elements;

public class Declaration : Element
{
    public Declaration() => Type = EElementType.Declaration;

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

        return viewRender.RenderAsync(Type.ToString(), this);
    }
}