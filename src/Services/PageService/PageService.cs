﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.ContentFactory.SuccessPageFactory;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.IncomingDataHelper;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.FileStorage;
using form_builder.Providers.StorageProvider;
using form_builder.Services.AddAnotherService;
using form_builder.Services.AddressService;
using form_builder.Services.BookingService;
using form_builder.Services.FileUploadService;
using form_builder.Services.FormAvailabilityService;
using form_builder.Services.OrganisationService;
using form_builder.Services.PageService.Entities;
using form_builder.Services.StreetService;
using form_builder.Validators;
using form_builder.ViewModels;
using form_builder.Workflows.ActionsWorkflow;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Services.PageService
{
    public class PageService : IPageService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IEnumerable<IFileStorageProvider> _fileStorageProviders;
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
        private readonly IBookingService _bookingService;
        private readonly IAddAnotherService _addAnotherService;
        private readonly ISuccessPageFactory _successPageContentFactory;
        private readonly IPageFactory _pageContentFactory;
        private readonly IIncomingDataHelper _incomingDataHelper;
        private readonly IActionsWorkflow _actionsWorkflow;
        private readonly IFormAvailabilityService _formAvailabilityServics;
        private readonly ILogger<IPageService> _logger;
        private readonly IConfiguration _configuration;

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
            IBookingService bookingService,
            ISchemaFactory schemaFactory,
            IIncomingDataHelper incomingDataHelper,
            IActionsWorkflow actionsWorkflow,
            IAddAnotherService addAnotherService,
            IFormAvailabilityService formAvailabilityServics,
            ILogger<IPageService> logger,
            IEnumerable<IFileStorageProvider> fileStorageProviders,
            IConfiguration configuration)
        {
            _validators = validators;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _streetService = streetService;
            _addressService = addressService;
            _bookingService = bookingService;
            _organisationService = organisationService;
            _fileUploadService = fileUploadService;
            _distributedCache = distributedCache;
            _schemaFactory = schemaFactory;
            _successPageContentFactory = successPageFactory;
            _pageContentFactory = pageFactory;
            _environment = environment;
            _formAvailabilityServics = formAvailabilityServics;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _incomingDataHelper = incomingDataHelper;
            _actionsWorkflow = actionsWorkflow;
            _logger = logger;
            _addAnotherService = addAnotherService;
            _fileStorageProviders = fileStorageProviders;
            _configuration = configuration;
        }

        public async Task<ProcessPageEntity> ProcessPage(string form, string path, string subPath, IQueryCollection queryParameters)
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

            if (baseForm == null)
                return null;

            if (!_formAvailabilityServics.IsAvailable(baseForm.EnvironmentAvailabilities, _environment.EnvironmentName))
            {
                _logger.LogWarning($"Form: {form} is not available in this Environment: {_environment.EnvironmentName.ToS3EnvPrefix()}");
                return null;
            }

            var formData = _distributedCache.GetString(sessionGuid);

            if (formData == null && path != baseForm.FirstPageSlug && (!baseForm.HasDocumentUpload || path != FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH))
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
                throw new ApplicationException($"Requested path '{path}' object could not be found for form '{form}'");

            List<object> searchResults = null;
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            if (subPath.Equals(LookUpConstants.Automatic) || subPath.Equals(LookUpConstants.Manual))
            {
                if (convertedAnswers.FormData.ContainsKey($"{path}{LookUpConstants.SearchResultsKeyPostFix}"))
                    searchResults = ((IEnumerable<object>)convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"])?.ToList();
            }

            if (page.HasIncomingGetValues)
            {
                var result = _incomingDataHelper.AddIncomingFormDataValues(page, queryParameters, convertedAnswers);
                _pageHelper.SaveNonQuestionAnswers(result, form, path, sessionGuid);
            }

            if (page.HasPageActionsGetValues)
                await _actionsWorkflow.Process(page.PageActions, null, form);

            if (page.Elements.Any(_ => _.Type.Equals(EElementType.Booking)))
            {
                var bookingProcessEntity = await _bookingService.Get(baseForm.BaseURL, page, sessionGuid);

                if (bookingProcessEntity.BookingHasNoAvailableAppointments)
                {
                    return new ProcessPageEntity
                    {
                        ShouldRedirect = true,
                        TargetPage = BookingConstants.NO_APPOINTMENT_AVAILABLE
                    };
                }

                searchResults = bookingProcessEntity.BookingInfo;
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
            IEnumerable<CustomFormFile> files,
            bool modelStateIsValid)
        {
            FormSchema baseForm = await _schemaFactory.Build(form);

            if (!_formAvailabilityServics.IsAvailable(baseForm.EnvironmentAvailabilities, _environment.EnvironmentName))
                throw new ApplicationException($"Form: {form} is not available in this Environment: {_environment.EnvironmentName.ToS3EnvPrefix()}");

            var currentPage = baseForm.GetPage(_pageHelper, path);

            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (sessionGuid == null)
                throw new NullReferenceException($"Session guid null.");

            if (currentPage == null)
                throw new NullReferenceException($"Current page '{path}' object could not be found.");

            if (currentPage.HasIncomingPostValues)
                viewModel = _incomingDataHelper.AddIncomingFormDataValues(currentPage, viewModel);

            var currentPageForValidation = currentPage;
            if (currentPage.Elements.Any(_ => !string.IsNullOrEmpty(_.Lookup) && _.Lookup.Equals("dynamic")))
            {
                currentPageForValidation = JsonConvert.DeserializeObject<Page>(JsonConvert.SerializeObject(currentPage));
                var formAnswers = _pageHelper.GetSavedAnswers(sessionGuid);
                var dynamicLookupElement = currentPageForValidation.Elements.FirstOrDefault(_ => _.Lookup.Equals("dynamic"));
                await _pageHelper.AddDynamicOptions(dynamicLookupElement, formAnswers);
            }

            currentPageForValidation.Validate(viewModel, _validators, baseForm);

            if (currentPageForValidation.Elements.Any(_ => _.Type == EElementType.AddAnother))
                return await _addAnotherService.ProcessAddAnother(viewModel, currentPageForValidation, baseForm, sessionGuid, path);

            if (currentPageForValidation.Elements.Any(_ => _.Type == EElementType.Address))
                return await _addressService.ProcessAddress(viewModel, currentPageForValidation, baseForm, sessionGuid, path);

            if (currentPageForValidation.Elements.Any(_ => _.Type == EElementType.Street))
                return await _streetService.ProcessStreet(viewModel, currentPageForValidation, baseForm, sessionGuid, path);

            if (currentPageForValidation.Elements.Any(_ => _.Type == EElementType.Organisation))
                return await _organisationService.ProcessOrganisation(viewModel, currentPageForValidation, baseForm, sessionGuid, path);

            if (currentPageForValidation.Elements.Any(_ => _.Type == EElementType.MultipleFileUpload))
                return await _fileUploadService.ProcessFile(viewModel, currentPageForValidation, baseForm, sessionGuid, path, files, modelStateIsValid);

            if (currentPageForValidation.Elements.Any(_ => _.Type == EElementType.Booking))
                return await _bookingService.ProcessBooking(viewModel, currentPageForValidation, baseForm, sessionGuid, path);

            _pageHelper.SaveAnswers(viewModel, sessionGuid, baseForm.BaseURL, files, currentPageForValidation.IsValid);

            if (!currentPageForValidation.IsValid)
            {
                var formModel = await _pageContentFactory.Build(currentPageForValidation, viewModel, baseForm, sessionGuid);

                return new ProcessRequestEntity
                {
                    Page = currentPageForValidation,
                    ViewModel = formModel
                };
            }

            return new ProcessRequestEntity
            {
                Page = currentPageForValidation
            };
        }

        public async Task<FormBuilderViewModel> GetViewModel(Page page, FormSchema baseForm, string path, string sessionGuid, string subPath, List<object> results)
        {
            var viewModelData = new Dictionary<string, dynamic>
            {
                { LookUpConstants.SubPathViewModelKey, subPath }
            };

            var viewModel = await _pageContentFactory.Build(page, viewModelData, baseForm, sessionGuid, null, results);

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
                throw new ApplicationException("PageService::FinalisePageJourney: Session has expired");

            var formData = _distributedCache.GetString(sessionGuid);

            if (formData == null)
                throw new ApplicationException("PageService::FinalisePageJourney: Session data is null");

            var formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            var formFileUploadElements = baseForm.Pages.SelectMany(_ => _.Elements)
                .Where(_ => _.Type == EElementType.FileUpload || _.Type == EElementType.MultipleFileUpload)
                .ToList();

            if (formFileUploadElements.Any())
                formFileUploadElements.ForEach(fileElement =>
                {
                    var formFileAnswerData = formAnswers.Pages.SelectMany(_ => _.Answers).FirstOrDefault(_ => _.QuestionId == $"{fileElement.Properties.QuestionId}{FileUploadConstants.SUFFIX}")?.Response ?? string.Empty;
                    List<FileUploadModel> convertedFileUploadAnswer = JsonConvert.DeserializeObject<List<FileUploadModel>>(formFileAnswerData.ToString());

                    var fileStorageType = _configuration["FileStorageProvider:Type"];
                    
                    var fileStorageProvider = _fileStorageProviders.Get(fileStorageType);

                    if (convertedFileUploadAnswer != null && convertedFileUploadAnswer.Any())
                    {
                        convertedFileUploadAnswer.ForEach((_) =>
                        {
                            fileStorageProvider.Remove(_.Key);
                        });
                    }
                });

            if (baseForm.DocumentDownload)
                await _distributedCache.SetStringAsync($"document-{sessionGuid}", JsonConvert.SerializeObject(formAnswers), _distributedCacheExpirationConfiguration.Document);

            return await _successPageContentFactory.Build(form, baseForm, sessionGuid, formAnswers, behaviourType);
        }

        public async Task<SuccessPageEntity> GetCancelBookingSuccessPage(string form)
        {
            var baseForm = await _schemaFactory.Build(form);
            var sessionGuid = _sessionHelper.GetSessionGuid();
            return await _successPageContentFactory.BuildBooking(form, baseForm, sessionGuid, new FormAnswers());
        }
    }
}