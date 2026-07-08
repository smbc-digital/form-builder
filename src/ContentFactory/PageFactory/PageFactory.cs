namespace form_builder.ContentFactory.PageFactory;

public class PageFactory(IPageHelper pageHelper,
    IEnumerable<ITagParser> tagParsers,
    IOptions<PreviewModeConfiguration> previewModeConfiguration,
    IDistributedCacheWrapper distributedCache)
    : IPageFactory
{
    private readonly IOptions<PreviewModeConfiguration> _previewModeConfiguration = previewModeConfiguration;

    public async Task<FormBuilderViewModel> Build(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string cacheKey, FormAnswers formAnswers = null, List<object> results = null)
    {
        if (formAnswers is null)
        {
            var cachedAnswers = distributedCache.GetString(cacheKey);

            formAnswers = cachedAnswers is null
                ? new FormAnswers { Pages = new List<PageAnswers>(), FormName = baseForm.BaseURL }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
        }

        foreach (var tagParser in tagParsers)
        {
            await tagParser.Parse(page, formAnswers, baseForm);
        }

        var result = await pageHelper.GenerateHtml(page, viewModel, baseForm, cacheKey, formAnswers, results);
        result.Path = page.PageSlug;
        result.FormName = baseForm.FormName;
        result.PageTitle = page.Title;
        result.FeedbackForm = baseForm.FeedbackForm;
        result.FeedbackPhase = baseForm.FeedbackPhase;
        result.HideBackButton = (viewModel.IsAutomatic() || viewModel.IsManual()) ? false : page.HideBackButton;
        result.BreadCrumbs = baseForm.BreadCrumbs;
        result.DisplayBreadCrumbs = page.DisplayBreadCrumbs;
        result.StartPageUrl = baseForm.StartPageUrl;
        result.Embeddable = baseForm.Embeddable;
        result.IsInPreviewMode = _previewModeConfiguration.Value.IsEnabled && baseForm.BaseURL.StartsWith(PreviewConstants.PREVIEW_MODE_PREFIX);
        return result;
    }
}