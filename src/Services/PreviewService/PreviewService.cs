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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PreviewService(
            IEnumerable<IElementValidator> validators,
            ISuccessPageFactory successPageFactory,
            IFileUploadService fileUploadService,
            IDistributedCacheWrapper distributedCache,
            ISchemaFactory schemaFactory,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            IOptions<PreviewModeConfiguration> previewModeConfiguration,
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
        }

        public async Task<FormBuilderViewModel> GetPreviewPage() 
        {
            if(!_previewModeConfiguration.PreviewMode)
                throw new ApplicationException("PreviewService: Request to access preview service recieved but preview service is disabled in current enviroment");
        
            return await _pageContentFactory.Build(PreviewPage(), new Dictionary<string, dynamic>(), new FormSchema(), string.Empty);
        }
        public void ExitPreviewMode()
        {
             if(!_previewModeConfiguration.PreviewMode)
                throw new ApplicationException("PreviewService: Request to exit preview mode recieved but preview service is disabled in current enviroment");

            var previewFormKey = _httpContextAccessor.HttpContext.Request.Cookies["PreviewMode"];
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("PreviewMode");
            _distributedCache.Remove($"{ESchemaType.FormJson.ToESchemaTypePrefix("v2")}{previewFormKey}");
        }

        public async Task<ProcessPreviewRequestEntity> VerifyPreviewRequest(IEnumerable<CustomFormFile> fileUpload)
        {
            if(!_previewModeConfiguration.PreviewMode)
                throw new ApplicationException("PreviewService: Request to upload from in preview service recieved but preview service is disabled in current enviroment");

            var viewModel = new Dictionary<string, dynamic>();

            if (fileUpload != null && fileUpload.Any())
                viewModel = _fileUploadService.AddFiles(viewModel, fileUpload);

            var previewPage = PreviewPage();
            previewPage.Validate(viewModel, _validators, new FormSchema { Pages = new List<Page> { previewPage } });

            if (!previewPage.IsValid)
            {
                var formModel = await _pageContentFactory.Build(previewPage, viewModel, new FormSchema { Pages = new List<Page> { previewPage } }, string.Empty);

                return new ProcessPreviewRequestEntity
                {
                    Page = previewPage,
                    ViewModel = formModel
                };
            }

            var previewKey = Guid.NewGuid();
            List<DocumentModel> fileContent = viewModel.Values.First();

            var t = Convert.FromBase64String(fileContent.First().Content);
            await _distributedCache.SetStringAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix("v2")}{previewKey}", t, _distributedCacheExpirationConfiguration.FormJson);

            try
            {
                var formSchema = await _schemaFactory.Build(previewKey.ToString(), true);
                formSchema.BaseURL = previewKey.ToString();
                await _distributedCache.SetStringAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix("v2")}{previewKey}", JsonConvert.SerializeObject(formSchema), _distributedCacheExpirationConfiguration.FormJson);
            }
            catch (Exception e)
            {
                _distributedCache.Remove($"{ESchemaType.FormJson.ToESchemaTypePrefix("v2")}{previewKey}");
                var errorPage = PreviewErrorPage(e.Message);
                var formModel = await _pageContentFactory.Build(errorPage, viewModel, new FormSchema { Pages = new List<Page> { errorPage } }, string.Empty);
                return new ProcessPreviewRequestEntity
                {
                    Page = errorPage,
                    ViewModel = formModel,
                    UseGeneratedViewModel = true
                };
            }

            var cookieOptions = new CookieOptions();
            cookieOptions.Secure = true;
            _httpContextAccessor.HttpContext.Response.Cookies.Append("PreviewMode", previewKey.ToString(), cookieOptions);

            return new ProcessPreviewRequestEntity
            {
                Page = previewPage,
                PreviewFormId = previewKey.ToString()
            };
        }

        private Page PreviewPage()
        {

            var fileUploadElement = new ElementBuilder()
                .WithQuestionId("previewFile")
                .WithType(Enum.EElementType.FileUpload)
                .WithLabelAsH1(true)
                .WithLabel("Preview Mode")
                .WithAcceptedMimeType(".json")
                .Build();

            var submitButton = new ElementBuilder()
                .WithType(Enum.EElementType.Button)
                .WithValue("Submit")
                .Build();

            return new PageBuilder()
                .WithElement(fileUploadElement)
                .WithElement(submitButton)
                .WithHideTitle(true)
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
                .Build();
        }

    }
}