using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.IncomingDataHelper;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.AddressService;
using form_builder.Services.FileUploadService;
using form_builder.Services.MappingService;
using form_builder.Services.OrganisationService;
using form_builder.Services.PageService.Entities;
using form_builder.Services.PayService;
using form_builder.Services.StreetService;
using form_builder.Validators;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
        private readonly IFileUploadService _fileUploadService;
        private readonly ISchemaFactory _schemaFactory;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly IWebHostEnvironment _environment;
        private readonly IPayService _payService;
        private readonly IMappingService _mappingService;
        private readonly ISuccessPageFactory _successPageContentFactory;
        private readonly IPageFactory _pageContentFactory;
        private readonly IIncomingDataHelper _incomingDataHelper;

        public PageService(
            IEnumerable<IElementValidator> validators, 
            IPageHelper pageHelper, 
            ISessionHelper sessionHelper, 
            IAddressService addressService,
            IFileUploadService fileUploadService,
            IStreetService streetService, 
            IOrganisationService organisationService, 
            IDistributedCacheWrapper distributedCache, 
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration, 
            IWebHostEnvironment environment, 
            ISuccessPageFactory successPageFactory,
            IPageFactory pageFactory,
            ISchemaFactory schemaFactory,
            IMappingService mappingService,
            IPayService payService,
            IIncomingDataHelper incomingDataHelper)
        {
            _validators = validators;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _streetService = streetService;
            _addressService = addressService;
            _organisationService = organisationService;
            _fileUploadService = fileUploadService;
            _distributedCache = distributedCache;
            _schemaFactory = schemaFactory;
            _successPageContentFactory = successPageFactory;
            _pageContentFactory = pageFactory;
            _environment = environment;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _payService = payService;
            _mappingService = mappingService;
            _incomingDataHelper = incomingDataHelper;
        }
        
        public async Task<ProcessPageEntity> ProcessPage(string form, string path, string subPath, IQueryCollection queryParamters)
        {
            if (string.IsNullOrEmpty(path))
                _sessionHelper.RemoveSessionGuid();

            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
            {
                sessionGuid = Guid.NewGuid().ToString();
                _sessionHelper.SetSessionGuid(sessionGuid);
            }

            var baseForm = await _schemaFactory.Build(form);

            if(!baseForm.IsAvailable(_environment.EnvironmentName))
                throw new ApplicationException($"Form: {form} is not available in this Environment: {_environment.EnvironmentName.ToS3EnvPrefix()}");

            var formData = _distributedCache.GetString(sessionGuid);

            if (formData == null && path != baseForm.FirstPageSlug)
                return new ProcessPageEntity
                {
                    ShouldRedirect = true,
                    TargetPage = baseForm.FirstPageSlug
                };

            if (string.IsNullOrEmpty(path))
                return new ProcessPageEntity
                {
                    ShouldRedirect = true,
                    TargetPage = baseForm.FirstPageSlug
                };

            if (formData != null && path == baseForm.FirstPageSlug)
            {
                var convertedFormData = JsonConvert.DeserializeObject<FormAnswers>(formData);
                if (form.ToLower() != convertedFormData.FormName.ToLower())
                    _distributedCache.Remove(sessionGuid);
            }

            var page = baseForm.GetPage(_pageHelper, path);
            if (page == null)
                throw new ApplicationException($"Requested path '{path}' object could not be found.");

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

            if (page.Elements.Any(_ => _.Type == EElementType.PaymentSummary))
            {
                var data = await _mappingService.Map(sessionGuid, form);
                var paymentAmount = await _payService.GetFormPaymentInformation(data, form, page);

                page.Elements.First(_ => _.Type == EElementType.PaymentSummary).Properties.Value = paymentAmount.Settings.Amount;
            }

            if(page.HasIncomingGetValues)
            {
                var result = _incomingDataHelper.AddIncomingFormDataValues(page, queryParamters);
                _pageHelper.SaveNonQuestionAnswers(result, form, path, sessionGuid);
            }

            var viewModel = await GetViewModel(page, baseForm, path, sessionGuid, subPath, searchResults);

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

            var currentPage = baseForm.GetPage(_pageHelper, path);

            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (sessionGuid == null)
                throw new NullReferenceException($"Session guid null.");

            if (currentPage == null)
                throw new NullReferenceException($"Current page '{path}' object could not be found.");

            if(currentPage.HasIncomingPostValues)
                viewModel = _incomingDataHelper.AddIncomingFormDataValues(currentPage, viewModel);

            currentPage.Validate(viewModel, _validators);

            if (currentPage.Elements.Any(_ => _.Type == EElementType.Address))
                return await _addressService.ProcessAddress(viewModel, currentPage, baseForm, sessionGuid, path);

            if (currentPage.Elements.Any(_ => _.Type == EElementType.Street))
                return await _streetService.ProcessStreet(viewModel, currentPage, baseForm, sessionGuid, path);

            if (currentPage.Elements.Any(_ => _.Type == EElementType.Organisation))
                return await _organisationService.ProcessOrganisation(viewModel, currentPage, baseForm, sessionGuid, path);

            if (currentPage.Elements.Any(_ => _.Type == EElementType.MultipleFileUpload))
                return await _fileUploadService.ProcessFile(viewModel, currentPage, baseForm, sessionGuid, path, files);
            
            _pageHelper.SaveAnswers(viewModel, sessionGuid, baseForm.BaseURL, files, currentPage.IsValid);

            if (!currentPage.IsValid)
            {
                var formModel = await _pageContentFactory.Build(currentPage, viewModel, baseForm, sessionGuid);

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
            var viewModelData = new Dictionary<string, dynamic>
            {
                { LookUpConstants.SubPathViewModelKey, subPath }
            };

            var viewModel = await _pageContentFactory.Build(page, viewModelData, baseForm, sessionGuid, results);

            return viewModel;
        }

        public Behaviour GetBehaviour(ProcessRequestEntity currentPageResult)
        {
            var answers = new Dictionary<string, dynamic>();

            var sessionGuid = _sessionHelper.GetSessionGuid();
            var cachedAnswers = _distributedCache.GetString(sessionGuid);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            convertedAnswers.Pages
                .SelectMany(_ => _.Answers)
                .ToList()
                .ForEach(x => answers.Add(x.QuestionId, x.Response));

            return currentPageResult.Page.GetNextPage(answers);
        }
        
        public async Task<SuccessPageEntity> FinalisePageJourney(string form, EBehaviourType behaviourType, FormSchema baseForm)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new Exception("PageService::FinalisePageJourney: Session has expired");

            var formData = _distributedCache.GetString(sessionGuid);
            var formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            var formFileUploadElements = baseForm.Pages.SelectMany(_ => _.Elements)
                .Where(_ => _.Type == EElementType.FileUpload || _.Type == EElementType.MultipleFileUpload)
                .ToList();

            if (formFileUploadElements.Any())
                formFileUploadElements.ForEach(fileElement =>
                {
                    var formFileAnswerData = formAnswers.Pages.SelectMany(_ => _.Answers).FirstOrDefault(_ => _.QuestionId == $"{fileElement.Properties.QuestionId}{FileUploadConstants.SUFFIX}")?.Response ?? string.Empty;
                    List<FileUploadModel> convertedFileUploadAnswer = JsonConvert.DeserializeObject<List<FileUploadModel>>(formFileAnswerData.ToString());

                    if(convertedFileUploadAnswer != null && convertedFileUploadAnswer.Any())
                    {
                        convertedFileUploadAnswer.ForEach((_) => {
                            _distributedCache.Remove(_.Key);
                        });
                    }
                });

            if(baseForm.DocumentDownload)
                await _distributedCache.SetStringAsync($"document-{sessionGuid}", JsonConvert.SerializeObject(formAnswers), _distributedCacheExpirationConfiguration.Document);

            return await _successPageContentFactory.Build(form, baseForm, sessionGuid, formAnswers, behaviourType);
        }
    }
}