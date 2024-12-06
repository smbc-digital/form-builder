using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace form_builder.Models.Elements;

public class UploadedFilesSummary : Element
{
    public UploadedFilesSummary() => Type = EElementType.UploadedFilesSummary;

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
        Properties.ClassName ??= "smbc-!-font-word-break";

        if (Properties.FileUploadQuestionIds.Any())
        {
            Properties.FileUploadQuestionIds.ForEach((questionId) =>
            {
                var model = elementHelper.CurrentValue<JArray>(questionId, viewModel, formAnswers, FileUploadConstants.SUFFIX);

                if (model is not null && model.Any())
                {
                    List<FileUploadModel> response = JsonConvert.DeserializeObject<List<FileUploadModel>>(model.ToString());
                    Properties.ListItems.AddRange(response.Select(_ => _.TrustedOriginalFileName));
                }
            });
        }

        return viewRender.RenderAsync(Type.ToString(), this);
    }
}