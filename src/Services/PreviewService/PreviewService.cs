namespace form_builder.Services.PreviewService;

public class PreviewService(IEnumerable<IElementValidator> validators,
    IFileUploadService fileUploadService,
    IDistributedCacheWrapper distributedCache,
    ISchemaFactory schemaFactory,
    IOptions<PreviewModeConfiguration> previewModeConfiguration,
    ICookieHelper cookieHelper,
    IPageFactory pageFactory)
    : IPreviewService
{
    private static int _expiryMinutes => 30;

    public async Task<FormBuilderViewModel> GetPreviewPage()
    {
        if (!previewModeConfiguration.Value.IsEnabled)
            throw new ApplicationException("PreviewService: Request to access preview service received but preview service is disabled in current environment");

        var previewPage = PreviewPage();
        return await pageFactory.Build(previewPage, new Dictionary<string, dynamic>(), PreviewModeFormSchema(previewPage), string.Empty, new FormAnswers());
    }

    public void ExitPreviewMode()
    {
        if (!previewModeConfiguration.Value.IsEnabled)
            throw new ApplicationException("PreviewService: Request to exit preview mode received but preview service is disabled in current environment");

        var cookiePreviewKeyValue = cookieHelper.GetCookie(CookieConstants.PREVIEW_MODE);
        cookieHelper.DeleteCookie(cookiePreviewKeyValue);
        distributedCache.Remove($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{cookiePreviewKeyValue}");
    }

    public async Task<ProcessPreviewRequestEntity> VerifyPreviewRequest(IEnumerable<CustomFormFile> fileUpload)
    {
        if (!previewModeConfiguration.Value.IsEnabled)
            throw new ApplicationException("PreviewService: Request to upload from in preview service received but preview service is disabled in current environment");

        var viewModel = new Dictionary<string, dynamic>();

        if (fileUpload is not null && fileUpload.Any())
            viewModel = fileUploadService.AddFiles(viewModel, fileUpload);

        var previewPage = PreviewPage();
        var previewFormSchema = PreviewModeFormSchema(previewPage);
        previewPage.Validate(viewModel, validators, previewFormSchema);

        if (!previewPage.IsValid)
        {
            var formModel = await pageFactory.Build(previewPage, viewModel, previewFormSchema, string.Empty, new FormAnswers());

            return new ProcessPreviewRequestEntity
            {
                Page = previewPage,
                ViewModel = formModel
            };
        }

        var previewKey = $"{PreviewConstants.PREVIEW_MODE_PREFIX}{Guid.NewGuid().ToString()}";
        List<DocumentModel> uploadedPreviewDocument = viewModel.Values.First();

        var fileContent = Convert.FromBase64String(uploadedPreviewDocument.First().Content);
        await distributedCache.SetAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{previewKey}", fileContent, new DistributedCacheEntryOptions { AbsoluteExpiration = DateTime.Now.AddMinutes(_expiryMinutes) });

        try
        {
            var formSchema = await schemaFactory.Build(previewKey);
            formSchema.BaseURL = previewKey;
            await distributedCache.SetStringAbsoluteAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{previewKey}", JsonConvert.SerializeObject(formSchema), _expiryMinutes);
        }
        catch (Exception e)
        {
            distributedCache.Remove($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{previewKey}");
            var errorPage = PreviewErrorPage(e.Message, e.Source);
            var errorSchema = PreviewModeFormSchema(errorPage);
            var formModel = await pageFactory.Build(errorPage, viewModel, errorSchema, string.Empty, new FormAnswers());
            return new ProcessPreviewRequestEntity
            {
                Page = errorPage,
                ViewModel = formModel,
                UseGeneratedViewModel = true
            };
        }

        cookieHelper.AddCookie(CookieConstants.PREVIEW_MODE, previewKey);

        return new ProcessPreviewRequestEntity
        {
            Page = previewPage,
            PreviewFormKey = previewKey.ToString()
        };
    }

    private FormSchema PreviewModeFormSchema(Page page) =>
        new FormSchema { Pages = new List<Page> { page }, FormName = "Preview", BaseURL = "preview" };
    private Page PreviewPage()
    {

        var fileUploadElement = new ElementBuilder()
            .WithQuestionId("previewFile")
            .WithType(EElementType.FileUpload)
            .WithIAG("Provide a txt/json file in the correct format to preview your form. Any errors will be detailed after upload if applicable.")
            .WithLabelAsH1(true)
            .WithLabel("Preview Mode")
            .WithAcceptedMimeType(".json")
            .WithAcceptedMimeType(".txt")
            .Build();

        var submitButton = new ElementBuilder()
            .WithType(EElementType.Button)
            .WithValue("Submit")
            .Build();

        return new PageBuilder()
            .WithElement(fileUploadElement)
            .WithElement(submitButton)
            .WithHideTitle(true)
            .WithPageTitle("Preview form request")
            .WithHideBackButton(true)
            .Build();
    }

    private Page PreviewErrorPage(string errorMessage, string errorSource)
    {
        var warning = new ElementBuilder()
            .WithType(EElementType.Warning)
            .WithPropertyText("The provided file is not valid.")
            .Build();

        if (errorSource.Equals(LibConstants.NEWTONSOFT_LIBRARY_NAME))
            errorMessage = $"The provided file is not valid JSON data format. Check the provided file JSON data structure. {SystemConstants.NEW_LINE_CHARACTER} {errorMessage}";

        string[] errorMessages = errorMessage.Split(SystemConstants.NEW_LINE_CHARACTER);

        var page = new PageBuilder()
            .WithElement(warning)
            .WithValidatedModel(true)
            .WithPageTitle("Preview mode");

        errorMessages.ToList().ForEach((error) =>
        {
            var pElement = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText(error)
                .Build();

            page.WithElement(pElement);
        });

        return page.Build();
    }
}