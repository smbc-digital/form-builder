namespace form_builder.Models.Elements;

public class Radio : Element
{
    public Radio() => Type = EElementType.Radio;

    public async override Task<string> RenderAsync(IViewRender viewRender,
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
        elementHelper.CheckForRadioOptions(this);
        elementHelper.ReCheckPreviousRadioOptions(this);
        elementHelper.OrderOptionsAlphabetically(this);

        return await viewRender.RenderAsync(Type.ToString(), this);
    }
}