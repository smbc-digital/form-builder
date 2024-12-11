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
using form_builder.TagParsers;
using form_builder.Validators;
using form_builder.ViewModels;
using form_builder.Workflows.ActionsWorkflow;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Services.PageService;

public class PageService : IPageService
{
    private readonly IDistributedCacheWrapper _distributedCache;
    private readonly IFileStorageProvider _fileStorageProvider;
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
    private readonly IFormAvailabilityService _formAvailabilityService;
    private readonly ILogger<IPageService> _logger;
    private readonly IEnumerable<ITagParser> _tagParsers;
    

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
        IFormAvailabilityService formAvailabilityService,
        ILogger<IPageService> logger,
        IEnumerable<IFileStorageProvider> fileStorageProviders,
        IEnumerable<ITagParser> tagParsers,
        IOptions<FileStorageProviderConfiguration> fileStorageConfiguration)
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
        _formAvailabilityService = formAvailabilityService;
        _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
        _incomingDataHelper = incomingDataHelper;
        _actionsWorkflow = actionsWorkflow;
        _logger = logger;
        _addAnotherService = addAnotherService;
        _tagParsers = tagParsers;
        _fileStorageProvider = fileStorageProviders.Get(fileStorageConfiguration.Value.Type);
    }

    public async Task<ProcessPageEntity> ProcessPage(string form, string path, string subPath, IQueryCollection queryParameters)
    {
        string browserSessionId = _sessionHelper.GetBrowserSessionId();
        
        if (string.IsNullOrEmpty(_sessionHelper.GetSessionFormName(form)))
            _sessionHelper.SetSessionFormName(form, "started");

        string cacheId = $"{form}::{browserSessionId}";

        _logger.LogInformation($"PageService:ProcessPage: Start processing page \"{form}/{path}/{subPath}\", Cache Id: {cacheId}");

        var formData = _distributedCache.GetString(cacheId);
        var pathIsEmpty = string.IsNullOrEmpty(path);
        var cacheIsEmpty = formData is null;

        if (pathIsEmpty)
        {
            _logger.LogInformation($"PageService:ProcessPage: New Cache created for {cacheId}");
            await _distributedCache.SetStringAsync(cacheId, JsonConvert.SerializeObject(new FormAnswers { Pages = new List<PageAnswers>() }));
        }

        if (cacheIsEmpty)
        {
            _logger.LogInformation($"PageService:ProcessPage: Cache Id was not found in Cache, new Cache created for {cacheId}");
            await _distributedCache.SetStringAsync(cacheId, JsonConvert.SerializeObject(new FormAnswers { Pages = new List<PageAnswers>() }));
        }

        var baseForm = await _schemaFactory.Build(form);
        if (baseForm is null)
        {
            _logger.LogWarning($"PageService:ProcessPage: Base form was null, Cache Id: {cacheId}");
            _distributedCache.Remove(cacheId);
            return null;
        }

        if (!_formAvailabilityService.IsAvailable(baseForm.EnvironmentAvailabilities, _environment.EnvironmentName))
        {
            _logger.LogWarning($"PageService:ProcessPage:Form {form} is not available in environment {_environment.EnvironmentName.ToS3EnvPrefix()}");
            _distributedCache.Remove(cacheId);
            return null;
        }

        if ((pathIsEmpty || cacheIsEmpty) && !_formAvailabilityService.IsFormAccessApproved(baseForm))
        {
            _logger.LogInformation($"PageService:ProcessPage:Access to {form} was not approved, Cache Id: {cacheId}");
            _distributedCache.Remove(cacheId);
            return null;
        }

        if (string.IsNullOrEmpty(path))
        {
            _logger.LogInformation($"PageService:ProcessPage:Path was empty, redirect to first page of form, Cache Id: {cacheId}");
            return new ProcessPageEntity { ShouldRedirect = true, TargetPage = baseForm.FirstPageSlug };
        }
                
        if (string.IsNullOrEmpty(formData) && !path.Equals(baseForm.FirstPageSlug) && (!baseForm.HasDocumentUpload || !path.Equals(FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH)))
        {
            _logger.LogInformation($"PageService:ProcessPage:Form data was empty and path was not the first page, redirect to first page of form, Cache Id: {cacheId}");
            return new ProcessPageEntity { ShouldRedirect = true, TargetPage = baseForm.FirstPageSlug };
        }

        if (!string.IsNullOrEmpty(formData) && path.Equals(baseForm.FirstPageSlug))
        {
            var convertedFormData = JsonConvert.DeserializeObject<FormAnswers>(formData);
            if (!string.IsNullOrEmpty(convertedFormData.FormName) && !form.Equals(convertedFormData.FormName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning($"PageService:ProcessPage: Disposing session as form names do not match {form}, {convertedFormData.FormName},  Cache Id: {cacheId}");
                _distributedCache.Remove(cacheId);
            }
        }

        var page = baseForm.GetPage(_pageHelper, path, form);
        if (page is null)
            throw new ApplicationException($"PageService:ProcessPage: Requested path {path} object could not be found in {form}, Cache Id: {cacheId}");

        List<object> searchResults = null;
        var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

        if (!string.IsNullOrEmpty(formData))
            convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

        if (page.Elements.Any(_ => _.Type.Equals(EElementType.Summary)))
        {
            var journeyPages = baseForm.GetReducedPages(convertedAnswers);
            foreach (var schemaPage in journeyPages)
                await _schemaFactory.TransformPage(schemaPage, convertedAnswers);
        }
        else
        {
            await _schemaFactory.TransformPage(page, convertedAnswers);
        }

        if (subPath.Equals(LookUpConstants.Automatic) || subPath.Equals(LookUpConstants.Manual))
        {
            if (convertedAnswers.FormData.ContainsKey($"{path}{LookUpConstants.SearchResultsKeyPostFix}"))
                searchResults = ((IEnumerable<object>)convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"])?.ToList();
        }

        if (page.HasIncomingGetValues)
        {
            var result = _incomingDataHelper.AddIncomingFormDataValues(page, queryParameters, convertedAnswers);
            _pageHelper.SaveNonQuestionAnswers(result, form, path, cacheId);
        }

        var validCaseRef = convertedAnswers.AdditionalFormData.ContainsKey(ValidateConstants.ValidateId);
        var actions = page.PageActions.Where(_ => _.Properties.HttpActionType.Equals(EHttpActionType.Get)).ToList();

        if (page.HasPageActionsGetValues)
        {
            if (actions.Any(_ => _.Type.Equals(EActionType.Validate)) && !validCaseRef ||
                !actions.Any(_ => _.Type.Equals(EActionType.Validate)))
                await _actionsWorkflow.Process(page.PageActions.Where(_ => _.Properties.HttpActionType.Equals(EHttpActionType.Get)).ToList(), null, form);
        }

        if (page.Elements.Any(_ => _.Type.Equals(EElementType.Booking)))
        {
            foreach (var tagParser in _tagParsers)
            {
                await tagParser.Parse(page, convertedAnswers);
            }

            var bookingProcessEntity = await _bookingService.Get(baseForm.BaseURL, page, cacheId);

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

        var viewModel = await GetViewModel(page, baseForm, path, cacheId, subPath, searchResults);
        _logger.LogInformation($"PageService:ProcessPage: Finish processing page \"{form}/{path}/{subPath}\", Cache Id: {cacheId}"); 
        return new ProcessPageEntity { ViewModel = viewModel };
    }

    public async Task<ProcessRequestEntity> ProcessRequest(
        string form,
        string path,
        Dictionary<string, dynamic> viewModel,
        IEnumerable<CustomFormFile> files,
        bool modelStateIsValid)
    {
        FormSchema baseForm = await _schemaFactory.Build(form);
        string browserSessionId = _sessionHelper.GetBrowserSessionId();
        string cacheId = $"{form}::{browserSessionId}";

        if (!_formAvailabilityService.IsAvailable(baseForm.EnvironmentAvailabilities, _environment.EnvironmentName))
            throw new ApplicationException($"PageService:ProcessRequest: {form} is not available in this Environment: {_environment.EnvironmentName.ToS3EnvPrefix()}, Cache Id: {cacheId}");

        if (browserSessionId is null)
            throw new NullReferenceException($"PageService:ProcessRequest: {form} Browser Session is null");

        var currentPage = baseForm.GetPage(_pageHelper, path, form);
        if (currentPage is null)
            throw new NullReferenceException($"PageService:ProcessRequest: {form} Current page '{path}' object could not be found, Cache Id: {cacheId}");

        var formData = _distributedCache.GetString(cacheId);
        var convertedAnswers = !string.IsNullOrEmpty(formData) ? JsonConvert.DeserializeObject<FormAnswers>(formData) : new FormAnswers { Pages = new List<PageAnswers>() };
        await _schemaFactory.TransformPage(currentPage, convertedAnswers);

        if (currentPage.HasIncomingPostValues)
            viewModel = _incomingDataHelper.AddIncomingFormDataValues(currentPage, viewModel);

        viewModel = _pageHelper.SanitizeViewModel(viewModel);
        currentPage.Validate(viewModel, _validators, baseForm);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.AddAnother)))
            return await _addAnotherService.ProcessAddAnother(viewModel, currentPage, baseForm, cacheId, path);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Address)))
            return await _addressService.ProcessAddress(viewModel, currentPage, baseForm, cacheId, path);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Street)))
            return await _streetService.ProcessStreet(viewModel, currentPage, baseForm, cacheId, path);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Organisation)))
            return await _organisationService.ProcessOrganisation(viewModel, currentPage, baseForm, cacheId, path);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.MultipleFileUpload)))
            return await _fileUploadService.ProcessFile(viewModel, currentPage, baseForm, cacheId, path, files, modelStateIsValid);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Booking)))
            return await _bookingService.ProcessBooking(viewModel, currentPage, baseForm, cacheId, path);

        _pageHelper.SaveAnswers(viewModel, cacheId, baseForm.BaseURL, files, currentPage.IsValid);

        if (!currentPage.IsValid)
        {
            var formModel = await _pageContentFactory.Build(currentPage, viewModel, baseForm, cacheId);
            return new ProcessRequestEntity { Page = currentPage, ViewModel = formModel };
        }

        return new ProcessRequestEntity { Page = currentPage };
    }

    public async Task<FormBuilderViewModel> GetViewModel(Page page, FormSchema baseForm, string path, string cacheKey, string subPath, List<object> results)
    {
        var viewModelData = new Dictionary<string, dynamic>
        {
            { LookUpConstants.SubPathViewModelKey, subPath }
        };

        var viewModel = await _pageContentFactory.Build(page, viewModelData, baseForm, cacheKey, null, results);

        return viewModel;
    }

    public async Task<Behaviour> GetBehaviour(ProcessRequestEntity currentPageResult, string form)
    {
        var answers = new Dictionary<string, dynamic>();
        string browserSessionId = _sessionHelper.GetBrowserSessionId();
        string formSessionId = $"{form}::{browserSessionId}";
        var cachedAnswers = _distributedCache.GetString(formSessionId);
        var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        convertedAnswers.Pages
            .SelectMany(_ => _.Answers)
            .ToList()
            .ForEach(x => answers.Add(x.QuestionId, x.Response));

        foreach (var tagParser in _tagParsers)
        {
            await tagParser.Parse(currentPageResult.Page, convertedAnswers);
        }

        return currentPageResult.Page.GetNextPage(answers);
    }

    public async Task<SuccessPageEntity> FinalisePageJourney(string form, EBehaviourType behaviourType, FormSchema baseForm)
    {
        string browserSessionId = _sessionHelper.GetBrowserSessionId();

        if (string.IsNullOrEmpty(browserSessionId))
            throw new ApplicationException($"PageService::FinalisePageJourney:{form} - Browser Session is null");

        string formSessionId = $"{form}::{browserSessionId}";

        var formData = _distributedCache.GetString(formSessionId);

        if (formData is null)
            throw new ApplicationException($"PageService::FinalisePageJourney: {formSessionId} Session data is null");

        var formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

        var formFileUploadElements = baseForm.Pages.SelectMany(_ => _.Elements)
            .Where(_ => _.Type.Equals(EElementType.FileUpload) || _.Type.Equals(EElementType.MultipleFileUpload))
            .ToList();

        if (formFileUploadElements.Any())
            formFileUploadElements.ForEach(fileElement =>
            {
                var formFileAnswerData = formAnswers.Pages.SelectMany(_ => _.Answers).FirstOrDefault(_ => _.QuestionId.Equals($"{fileElement.Properties.QuestionId}{FileUploadConstants.SUFFIX}"))?.Response ?? string.Empty;
                List<FileUploadModel> convertedFileUploadAnswer = JsonConvert.DeserializeObject<List<FileUploadModel>>(formFileAnswerData.ToString());

                if (convertedFileUploadAnswer is not null && convertedFileUploadAnswer.Any())
                {
                    convertedFileUploadAnswer.ForEach((_) =>
                    {
                        _fileStorageProvider.Remove(_.Key);
                    });
                }
            });

        if (baseForm.Pages.Where(_ => _.PageSlug.ToLower().Equals("success")).Any() && baseForm.GetPage(_pageHelper, "success", form).Elements.Where(_ => _.Type.Equals(EElementType.DocumentDownload)).Any())
            await _distributedCache.SetStringAsync($"document-{formSessionId}", JsonConvert.SerializeObject(formAnswers), _distributedCacheExpirationConfiguration.Document);

        return await _successPageContentFactory.Build(form, baseForm, formSessionId, formAnswers, behaviourType);
    }

    public async Task<SuccessPageEntity> GetCancelBookingSuccessPage(string form)
    {
        var baseForm = await _schemaFactory.Build(form);
        string browserSessionId = _sessionHelper.GetBrowserSessionId();
        string formSessionId = $"{form}::{browserSessionId}";
        return await _successPageContentFactory.BuildBooking(form, baseForm, formSessionId, new FormAnswers());
    }
}