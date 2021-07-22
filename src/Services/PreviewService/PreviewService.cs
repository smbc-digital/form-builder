using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.ContentFactory.PageFactory;
using form_builder.Models;
using form_builder.ViewModels;
using form_builder.Validators;
using form_builder.Services.PageService.Entities;
using System.Linq;
using form_builder.Services.FileUploadService;
using form_builder.Providers.StorageProvider;
using form_builder.Configuration;
using System;
using form_builder.Enum;
using form_builder.Extensions;
using Microsoft.Extensions.Options;
using form_builder.Factories.Schema;
using Newtonsoft.Json;
using form_builder.Constants;
using Microsoft.Extensions.Caching.Distributed;
using form_builder.Helpers.Cookie;

namespace form_builder.Services.PreviewService
{
    public class PreviewService : IPreviewService
    {
        private readonly IEnumerable<IElementValidator> _validators;
        private readonly IPageFactory _pageContentFactory;
        private readonly IFileUploadService _fileUploadService;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly ISchemaFactory _schemaFactory;
        private readonly IOptions<PreviewModeConfiguration> _previewModeConfiguration;
        private readonly ApplicationVersionConfiguration _applicationVersionConfiguration;
        private readonly ICookieHelper _cookieHelper;
        private int _expiryMinutes => 30;

        public PreviewService(
            IEnumerable<IElementValidator> validators,
            IFileUploadService fileUploadService,
            IDistributedCacheWrapper distributedCache,
            ISchemaFactory schemaFactory,
            IOptions<PreviewModeConfiguration> previewModeConfiguration,
            IOptions<ApplicationVersionConfiguration> applicationVersionConfiguration,
            ICookieHelper cookieHelper,
            IPageFactory pageFactory)
        {
            _validators = validators;
            _pageContentFactory = pageFactory;
            _fileUploadService = fileUploadService;
            _distributedCache = distributedCache;
            _schemaFactory = schemaFactory;
            _cookieHelper = cookieHelper;
            _previewModeConfiguration = previewModeConfiguration;
            _applicationVersionConfiguration = applicationVersionConfiguration.Value;
        }

        public async Task<FormBuilderViewModel> GetPreviewPage()
        {
            if (!_previewModeConfiguration.Value.IsEnabled)
                throw new ApplicationException("PreviewService: Request to access preview service recieved but preview service is disabled in current enviroment");

            var previewPage = PreviewPage();
            return await _pageContentFactory.Build(previewPage, new Dictionary<string, dynamic>(), PreviewModeFormSchema(previewPage), string.Empty, new FormAnswers());
        }

        public void ExitPreviewMode()
        {
            if (!_previewModeConfiguration.Value.IsEnabled)
                throw new ApplicationException("PreviewService: Request to exit preview mode recieved but preview service is disabled in current enviroment");

            var cookiePreviewKeyValue = _cookieHelper.GetCookie(CookieConstants.PREVIEW_MODE);
            _cookieHelper.DeleteCookie(cookiePreviewKeyValue);
            _distributedCache.Remove($"{ESchemaType.FormJson.ToESchemaTypePrefix(_applicationVersionConfiguration.Version)}{cookiePreviewKeyValue}");
        }

        public async Task<ProcessPreviewRequestEntity> VerifyPreviewRequest(IEnumerable<CustomFormFile> fileUpload)
        {
            if (!_previewModeConfiguration.Value.IsEnabled)
                throw new ApplicationException("PreviewService: Request to upload from in preview service recieved but preview service is disabled in current enviroment");

            var viewModel = new Dictionary<string, dynamic>();

            if (fileUpload != null && fileUpload.Any())
                viewModel = _fileUploadService.AddFiles(viewModel, fileUpload);

            var previewPage = PreviewPage();
            var previewFormSchema = PreviewModeFormSchema(previewPage);
            previewPage.Validate(viewModel, _validators, previewFormSchema);

            if (!previewPage.IsValid)
            {
                var formModel = await _pageContentFactory.Build(previewPage, viewModel, previewFormSchema, string.Empty, new FormAnswers());

                return new ProcessPreviewRequestEntity
                {
                    Page = previewPage,
                    ViewModel = formModel
                };
            }

            var previewKey = $"{PreviewConstants.PREVIEW_MODE_PREFIX}{Guid.NewGuid().ToString()}";
            List<DocumentModel> uploadedPreviewDocument = viewModel.Values.First();

            var fileContent = Convert.FromBase64String(uploadedPreviewDocument.First().Content);
            await _distributedCache.SetAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix(_applicationVersionConfiguration.Version)}{previewKey}", fileContent, new DistributedCacheEntryOptions { AbsoluteExpiration = DateTime.Now.AddMinutes(_expiryMinutes) });

            try
            {
                var formSchema = await _schemaFactory.Build(previewKey);
                formSchema.BaseURL = previewKey;
                await _distributedCache.SetStringAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix(_applicationVersionConfiguration.Version)}{previewKey}", JsonConvert.SerializeObject(formSchema), _expiryMinutes);
            }
            catch (Exception e)
            {
                _distributedCache.Remove($"{ESchemaType.FormJson.ToESchemaTypePrefix(_applicationVersionConfiguration.Version)}{previewKey}");
                var errorPage = PreviewErrorPage(e.Message);
                var errorSchema = PreviewModeFormSchema(errorPage);
                var formModel = await _pageContentFactory.Build(errorPage, viewModel, errorSchema, string.Empty, new FormAnswers());
                return new ProcessPreviewRequestEntity
                {
                    Page = errorPage,
                    ViewModel = formModel,
                    UseGeneratedViewModel = true
                };
            }

            _cookieHelper.AddCookie(CookieConstants.PREVIEW_MODE, previewKey);

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
        
        private Page PreviewErrorPage(string errorMessage)
        {
            var warning = new ElementBuilder()
                .WithType(EElementType.Warning)
                .WithPropertyText("The provided file is not valid.")
                .Build();

            string[] errorMessages = errorMessage.Split("\n");

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
}