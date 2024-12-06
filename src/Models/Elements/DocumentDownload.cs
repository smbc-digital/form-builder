using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;

namespace form_builder.Models.Elements;

public class DocumentDownload : Element
{
    public DocumentDownload() => Type = EElementType.DocumentDownload;

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
        Properties.Text = string.IsNullOrEmpty(Properties.Text) ? $"Download {Properties.DocumentType} document" : Properties.Text;
        Properties.Source = $"/document/Summary/{Properties.DocumentType}/{cacheKey}";
        return viewRender.RenderAsync("DocumentDownload", this);
    }
}