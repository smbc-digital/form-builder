using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace form_builder.Models.Elements;

public class MultipleFileUpload : Element
{
    public MultipleFileUpload() => Type = EElementType.MultipleFileUpload;

    public string AllowFileTypeText { get { return Properties.AllowedFileTypes?.ToReadableFileType("and") ?? SystemConstants.AcceptedMimeTypes.ToReadableFileType("and"); } }
    public string MaxFileSizeText { get { return $"{((Properties.MaxFileSize * SystemConstants.OneMBInBinaryBytes).Equals(0) ? SystemConstants.DefaultMaxFileSize.ToReadableMaxFileSize() : Properties.MaxFileSize)}MB"; } }
    public string MaxCombinedFileSizeText { get { return $"{(Properties.MaxCombinedFileSize.Equals(0) ? SystemConstants.DefaultMaxCombinedFileSize.ToReadableMaxFileSize() : Properties.MaxCombinedFileSize)}MB"; } }
    public override string QuestionId => $"{base.QuestionId}{FileUploadConstants.SUFFIX}";
    public List<string> CurrentFilesUploaded { get; set; } = new List<string>();
    public int MaxFileSize => Properties.MaxFileSize * SystemConstants.OneMBInBinaryBytes > 0 && Properties.MaxFileSize * SystemConstants.OneMBInBinaryBytes < SystemConstants.DefaultMaxFileSize ? Properties.MaxFileSize * SystemConstants.OneMBInBinaryBytes : SystemConstants.DefaultMaxFileSize;
    public bool DisplaySubmitButton => (CurrentFilesUploaded.Any() && !Properties.Optional) || Properties.Optional;

    public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
    {
        var allowedFileType = Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;

        var properties = new Dictionary<string, dynamic>
        {
            { "name", QuestionId },
            { "id", QuestionId },
            { "type", "file" },
            { "multiple", true },
            { "accept", string.Join(',', allowedFileType)},
            { "data-module", "smbc-multiple-file-upload" },
            { "data-individual-file-size", MaxFileSize }
        };

        if (DisplayAriaDescribedby)
            properties.Add("aria-describedby", GetDescribedByAttributeValue());

        return properties;
    }

    public string SubmitButtonText;

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
        var currentAnswer = elementHelper.CurrentValue<JArray>(Properties.QuestionId, viewModel, formAnswers, FileUploadConstants.SUFFIX);

        SubmitButtonText = SetSubmitButtonText(page);
        IsModelStateValid = !viewModel.ContainsKey("modelStateInvalid");

        if (currentAnswer is not null)
        {
            List<FileUploadModel> response = JsonConvert.DeserializeObject<List<FileUploadModel>>(currentAnswer.ToString());
            CurrentFilesUploaded = response.Select(_ => _.TrustedOriginalFileName).ToList();
        }

        Properties.HideOptionalText = true;

        elementHelper.CheckForQuestionId(this);
        elementHelper.CheckForLabel(this);

        return viewRender.RenderAsync(Type.ToString(), this);
    }

    private string SetSubmitButtonText(Page page)
    {
        if (page.Behaviours.Any(_ => _.BehaviourType.Equals(EBehaviourType.SubmitForm) || _.BehaviourType.Equals(EBehaviourType.SubmitAndPay)))
            return string.IsNullOrEmpty(Properties.PageSubmitButtonLabel) ? ButtonConstants.SUBMIT_TEXT : Properties.PageSubmitButtonLabel;

        return string.IsNullOrEmpty(Properties.PageSubmitButtonLabel) ? ButtonConstants.NEXTSTEP_TEXT : Properties.PageSubmitButtonLabel;
    }
}