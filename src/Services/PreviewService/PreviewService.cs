using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.ContentFactory.PageFactory;
using form_builder.ContentFactory.SuccessPageFactory;
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
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using form_builder.Constants;

namespace form_builder.Services.PreviewService
{
    public interface IPreviewService
    {
        Task<FormBuilderViewModel> GetPreviewPage();
        void ExitPreviewMode();
        Task<ProcessPreviewRequestEntity> VerifyPreviewRequest(IEnumerable<CustomFormFile> fileUpload);
    }

    public class PreviewService : IPreviewService
    {
        private readonly IEnumerable<IElementValidator> _validators;
        private readonly ISuccessPageFactory _successPageContentFactory;
        private readonly IPageFactory _pageContentFactory;
        private readonly IFileUploadService _fileUploadService;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly ISchemaFactory _schemaFactory;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly PreviewModeConfiguration _previewModeConfiguration;
        private readonly ApplicationVersionConfiguration _applicationVersionConfiguration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PreviewService(
            IEnumerable<IElementValidator> validators,
            ISuccessPageFactory successPageFactory,
            IFileUploadService fileUploadService,
            IDistributedCacheWrapper distributedCache,
            ISchemaFactory schemaFactory,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            IOptions<PreviewModeConfiguration> previewModeConfiguration,
            IOptions<ApplicationVersionConfiguration> applicationVersionConfiguration,
            IHttpContextAccessor httpContextAccessor,
            IPageFactory pageFactory)
        {
            _validators = validators;
            _successPageContentFactory = successPageFactory;
            _pageContentFactory = pageFactory;
            _fileUploadService = fileUploadService;
            _distributedCache = distributedCache;
            _schemaFactory = schemaFactory;
            _httpContextAccessor = httpContextAccessor;
            _previewModeConfiguration = previewModeConfiguration.Value;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _applicationVersionConfiguration = applicationVersionConfiguration.Value;
        }

        public async Task<FormBuilderViewModel> GetPreviewPage() 
        {
            if(!_previewModeConfiguration.IsEnabled)
                throw new ApplicationException("PreviewService: Request to access preview service recieved but preview service is disabled in current enviroment");
        
            var previewPage = PreviewPage();
            return await _pageContentFactory.Build(previewPage, new Dictionary<string, dynamic>(), PreviewModeFormSchema(previewPage), string.Empty, new FormAnswers());
        }
        
        public void ExitPreviewMode()
        {
             if(!_previewModeConfiguration.IsEnabled)
                throw new ApplicationException("PreviewService: Request to exit preview mode recieved but preview service is disabled in current enviroment");

            var previewFormKey = _httpContextAccessor.HttpContext.Request.Cookies["PreviewMode"];
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("PreviewMode");
            _distributedCache.Remove($"{ESchemaType.FormJson.ToESchemaTypePrefix(_applicationVersionConfiguration.Version)}{previewFormKey}");
        }

        public async Task<ProcessPreviewRequestEntity> VerifyPreviewRequest(IEnumerable<CustomFormFile> fileUpload)
        {
            if(!_previewModeConfiguration.IsEnabled)
                throw new ApplicationException("PreviewService: Request to upload from in preview service recieved but preview service is disabled in current enviroment");

            var viewModel = new Dictionary<string, dynamic>();

            if (fileUpload != null && fileUpload.Any())
                viewModel = _fileUploadService.AddFiles(viewModel, fileUpload);

            var previewPage = PreviewPage();
            previewPage.Validate(viewModel, _validators, PreviewModeFormSchema(PreviewPage()));

            if (!previewPage.IsValid)
            {
                var formModel = await _pageContentFactory.Build(previewPage, viewModel, PreviewModeFormSchema(PreviewPage()), string.Empty, new FormAnswers());

                return new ProcessPreviewRequestEntity
                {
                    Page = previewPage,
                    ViewModel = formModel
                };
            }

            var previewKey = $"{PreviewConstants.PREVIEW_MODE_PREFIX}{Guid.NewGuid().ToString()}";
            List<DocumentModel> uploadedPreviewDocument = viewModel.Values.First();

            var fileContent = Convert.FromBase64String(uploadedPreviewDocument.First().Content);
            await _distributedCache.SetStringAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix(_applicationVersionConfiguration.Version)}{previewKey}", fileContent, _distributedCacheExpirationConfiguration.FormJson);

            try
            {
                var formSchema = await _schemaFactory.Build(previewKey);
                formSchema.BaseURL = previewKey;
                await _distributedCache.SetStringAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix(_applicationVersionConfiguration.Version)}{previewKey}", JsonConvert.SerializeObject(formSchema), _distributedCacheExpirationConfiguration.FormJson);
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

            var cookieOptions = new CookieOptions();
            cookieOptions.Secure = true;
            _httpContextAccessor.HttpContext.Response.Cookies.Append("PreviewMode", previewKey, cookieOptions);

            return new ProcessPreviewRequestEntity
            {
                Page = previewPage,
                PreviewFormId = previewKey.ToString()
            };
        }

        private FormSchema PreviewModeFormSchema(Page page) =>
            new FormSchema { Pages = new List<Page>{ page }, FormName = "Preview", BaseURL = "preview" };

        private Page PreviewPage()
        {

            var fileUploadElement = new ElementBuilder()
                .WithQuestionId("previewFile")
                .WithType(Enum.EElementType.FileUpload)
                .WithLabelAsH1(true)
                .WithLabel("Preview Mode")
                .WithAcceptedMimeType(".json")
                .WithAcceptedMimeType(".txt")
                .Build();

            var submitButton = new ElementBuilder()
                .WithType(Enum.EElementType.Button)
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
            var pElement = new ElementBuilder()
                .WithType(Enum.EElementType.P)
                .WithPropertyText(errorMessage)
                .Build();

            return new PageBuilder()
                .WithElement(pElement)
                .WithValidatedModel(true)
                .WithPageTitle("Preview form request")
                .Build();
        }

    }
}