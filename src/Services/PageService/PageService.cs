using System.Collections.Generic;
using form_builder.Models;
using form_builder.Validators;
using System.Threading.Tasks;
using form_builder.Helpers.PageHelpers;
using Microsoft.Extensions.Logging;
using form_builder.Helpers.Session;
using System;
using form_builder.Services.PageService.Entities;
using System.Linq;
using form_builder.Enum;
using form_builder.Services.AddressService;
using form_builder.Services.StreetService;
using form_builder.ViewModels;
using form_builder.Providers.StorageProvider;
using form_builder.Extensions;
using Newtonsoft.Json;
using form_builder.Services.OrganisationService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using form_builder.Configuration;
using Microsoft.Extensions.Options;
using form_builder.ContentFactory;
using form_builder.Factories.Schema;
using form_builder.Constants;
using form_builder.Services.MappingService;
using form_builder.Services.PayService;

namespace form_builder.Services.PageService
{
    public class PageService : IPageService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IEnumerable<IElementValidator> _validators;
        private readonly IPageHelper _pageHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly IStreetService _streetService;
        private readonly IAddressService _addressService;
        private readonly IOrganisationService _organisationService;
        private readonly ISchemaFactory _schemaFactory;
        private readonly DistributedCacheExpirationConfiguration _distrbutedCacheExpirationConfiguration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _environment;
        private readonly ISuccessPageContentFactory _successPageContentFactory;
        private readonly IPayService _payService;
        private readonly IMappingService _mappingService;

        public PageService(
            IEnumerable<IElementValidator> validators, 
            IPageHelper pageHelper, 
            ISessionHelper sessionHelper, 
            IAddressService addressService, 
            IStreetService streetService, 
            IOrganisationService organisationService, 
            IDistributedCacheWrapper distributedCache, 
            IOptions<DistributedCacheExpirationConfiguration> distrbutedCacheExpirationConfiguration, 
            IHttpContextAccessor httpContextAccessor, 
            IHostingEnvironment environment, 
            ISuccessPageContentFactory successPageContentFactory, 
            ISchemaFactory schemaFactory,
            IPayService payService,
            IMappingService mappingService)
        {
            _validators = validators;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _streetService = streetService;
            _addressService = addressService;
            _organisationService = organisationService;
            _distributedCache = distributedCache;
            _schemaFactory = schemaFactory;
            _successPageContentFactory = successPageContentFactory;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _distrbutedCacheExpirationConfiguration = distrbutedCacheExpirationConfiguration.Value;
            _payService = payService;
            _mappingService = mappingService;
        }
        
        public async Task<ProcessPageEntity> ProcessPage(string form, string path, string subPath)
        {
            if (string.IsNullOrEmpty(path))
            {
                _sessionHelper.RemoveSessionGuid();
            }

            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
            {
                sessionGuid = Guid.NewGuid().ToString();
                _sessionHelper.SetSessionGuid(sessionGuid);
            }

            var baseForm = await _schemaFactory.Build(form);

            if(!baseForm.IsAvailable(_environment.EnvironmentName))
            {
                throw new ApplicationException($"Form: {form} is not available in this Environment: {_environment.EnvironmentName.ToS3EnvPrefix()}");
            }

            var formData = _distributedCache.GetString(sessionGuid);

            if (formData == null && path != baseForm.StartPageSlug)
            {
                return new ProcessPageEntity
                {
                    ShouldRedirect = true,
                    TargetPage = baseForm.StartPageSlug
                };
            }

            if (string.IsNullOrEmpty(path))
            {
                return new ProcessPageEntity
                {
                    ShouldRedirect = true,
                    TargetPage = baseForm.StartPageSlug
                };
            }

            if (formData != null && path == baseForm.StartPageSlug)
            {
                var convertedFormData = JsonConvert.DeserializeObject<FormAnswers>(formData);
                if (form.ToLower() != convertedFormData.FormName.ToLower())
                    _distributedCache.Remove(sessionGuid);
            }

            var page = baseForm.GetPage(path);
            if (page == null)
            {
                throw new ApplicationException($"Requested path '{path}' object could not be found.");
            }

            await baseForm.ValidateFormSchema(_pageHelper, form, path);

            List<object> searchResults = null;
            if (subPath.Equals(LookUpConstants.Automatic) || subPath.Equals(LookUpConstants.Manual))
            {
                var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

                if (!string.IsNullOrEmpty(formData))
                    convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

                if(convertedAnswers.FormData.ContainsKey($"{path}{LookUpConstants.SearchResultsKeyPostFix}"))
                    searchResults = ((IEnumerable<object>)convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"])?.ToList();
            }

            if (page.PageSlug == "payment-summary")
            {
                var data = await _mappingService.Map(sessionGuid, form);
                var paymentAmount = await _payService.GetFormPaymentInformation(data, form, page);

                foreach (var element in page.Elements.Where(_ => _.Type == EElementType.PaymentSummary))
                {
                    element.Properties.PaymentAmount = paymentAmount.Settings.Amount;
                }
            }

            var viewModel = await GetViewModel(page, baseForm, path, sessionGuid, subPath, searchResults);
            viewModel.StartFormUrl = $"https://{_httpContextAccessor.HttpContext.Request.Host}/{viewModel.BaseURL}/{viewModel.StartPageSlug}";

            return new ProcessPageEntity
            {
                ViewModel = viewModel
            };
        }

        public async Task<ProcessRequestEntity> ProcessRequest(
            string form,
            string path,
            Dictionary<string, dynamic> viewModel,
            IEnumerable<CustomFormFile> files)
        {
            var baseForm = await _schemaFactory.Build(form);

            if(!baseForm.IsAvailable(_environment.EnvironmentName))
                throw new ApplicationException($"Form: {form} is not available in this Environment: {_environment.EnvironmentName.ToS3EnvPrefix()}");

            var currentPage = baseForm.GetPage(path);

            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (sessionGuid == null)
                throw new NullReferenceException($"Session guid null.");

            if (currentPage == null)
                throw new NullReferenceException($"Current page '{path}' object could not be found.");

            if(currentPage.HasIncomingValues)
                viewModel = _pageHelper.AddIncomingFormDataValues(currentPage, viewModel);

            currentPage.Validate(viewModel, _validators);

            if (currentPage.Elements.Any(_ => _.Type == EElementType.Address))
                return await _addressService.ProcessAddress(viewModel, currentPage, baseForm, sessionGuid, path);

            if (currentPage.Elements.Any(_ => _.Type == EElementType.Street))
                return await _streetService.ProcessStreet(viewModel, currentPage, baseForm, sessionGuid, path);

            if (currentPage.Elements.Any(_ => _.Type == EElementType.Organisation))
                return await _organisationService.ProcessOrganisation(viewModel, currentPage, baseForm, sessionGuid, path);

            _pageHelper.SaveAnswers(viewModel, sessionGuid, baseForm.BaseURL, files, currentPage.IsValid);

            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, sessionGuid);
                formModel.Path = currentPage.PageSlug;
                formModel.FormName = baseForm.FormName;
                formModel.PageTitle = currentPage.Title;
                formModel.HideBackButton = currentPage.HideBackButton;
                formModel.BaseURL = baseForm.BaseURL;
                formModel.StartPageSlug = baseForm.StartPageSlug;

                var startFormUrl = $"https://{_httpContextAccessor.HttpContext.Request.Host}/{formModel.BaseURL}/{formModel.StartPageSlug}";
                formModel.StartFormUrl = startFormUrl;

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = formModel
                };
            }

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        public async Task<FormBuilderViewModel> GetViewModel(Page page, FormSchema baseForm, string path, string sessionGuid, string subPath, List<object> results)
        {
            var viewModelData = new Dictionary<string, dynamic>();
            viewModelData.Add(LookUpConstants.SubPathViewModelKey, subPath);

            var viewModel = await _pageHelper.GenerateHtml(page, viewModelData, baseForm, sessionGuid, results);
            viewModel.FormName = baseForm.FormName;
            viewModel.PageTitle = page.Title;
            viewModel.HideBackButton = page.HideBackButton;
            viewModel.Path = path;
            viewModel.BaseURL = baseForm.BaseURL;
            viewModel.StartPageSlug = baseForm.StartPageSlug;

            return viewModel;
        }

        public Behaviour GetBehaviour(ProcessRequestEntity currentPageResult)
        {
            Dictionary<string, dynamic> answers = new Dictionary<string, dynamic>();

            var sessionGuid = _sessionHelper.GetSessionGuid();
            var cachedAnswers = _distributedCache.GetString(sessionGuid);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            convertedAnswers.Pages
                .SelectMany(_ => _.Answers)
                .ToList()
                .ForEach(x => answers.Add(x.QuestionId, x.Response));

            return currentPageResult.Page.GetNextPage(answers);
        }
        
        public async Task<SuccessPageEntity> FinalisePageJourney(string form, EBehaviourType behaviourType)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
            {
                throw new Exception("PageService::FinalisePageJourney: Session has expired");
            }

            var formData = _distributedCache.GetString(sessionGuid);
            var formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            var baseForm = await _schemaFactory.Build(form);

            var formFileUploadElements = baseForm.Pages.SelectMany(_ => _.Elements)
                .Where(_ => _.Type == EElementType.FileUpload)
                .ToList();

            if (formFileUploadElements.Count > 0)
            {
                formFileUploadElements.ForEach(_ =>
                {
                    _distributedCache.Remove($"file-{_.Properties.QuestionId}-fileupload-{sessionGuid}");
                });
            }

            if(baseForm.DocumentDownload)
                await _distributedCache.SetStringAsync($"document-{sessionGuid}", JsonConvert.SerializeObject(formAnswers), _distrbutedCacheExpirationConfiguration.Document);

            _distributedCache.Remove(sessionGuid);
            _sessionHelper.RemoveSessionGuid();

            return await _successPageContentFactory.Build(form, baseForm, sessionGuid, formAnswers, behaviourType);
        }
    }
}