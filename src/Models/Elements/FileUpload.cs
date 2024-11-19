using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;

namespace form_builder.Models.Elements;

public class FileUpload : Element
{
    public FileUpload() => Type = EElementType.FileUpload;

    public override string QuestionId => $"{base.QuestionId}{FileUploadConstants.SUFFIX}";

    public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
    {
        var allowedFileType = Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;
        var convertedMaxFileSize = Properties.MaxFileSize * SystemConstants.OneMBInBinaryBytes;
        var appliedMaxFileSize = convertedMaxFileSize > 0 && convertedMaxFileSize < SystemConstants.DefaultMaxFileSize
            ? convertedMaxFileSize
            : SystemConstants.DefaultMaxFileSize;

        var properties = new Dictionary<string, dynamic>
        {
            { "name", QuestionId },
            { "id", QuestionId },
            { "type", "file" },
            { "accept", string.Join(',', allowedFileType)},
            { "max-file-size", appliedMaxFileSize },
            { "data-module", "smbc-file-upload" }
        };

        if (DisplayAriaDescribedby)
            properties.Add("aria-describedby", GetDescribedByAttributeValue());

        return properties;
    }

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
        elementHelper.CheckForQuestionId(this);
        elementHelper.CheckForLabel(this);

        return viewRender.RenderAsync(Type.ToString(), this);
    }
}