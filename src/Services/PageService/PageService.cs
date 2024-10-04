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

namespace form_builder.Services.PageService
{
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
            var session = _sessionHelper.GetSession();

            _logger.LogInformation($"PageService:ProcessPage: Start processing page \"{form}/{path}/{subPath}\", Browser Session:{session.Id}");

            var isNewSession = false;
            var currentForm = _sessionHelper.GetSessionForm(); 
                                
            if(session is null)
            {
                _logger.LogInformation($"PageService:ProcessPage: Browser session was empty {form} {path} {subPath}");
            }

            if (string.IsNullOrEmpty(path) || (!string.IsNullOrEmpty(currentForm) && !form.Equals(currentForm)))
            {
                _logger.LogInformation($"PageService:ProcessPage: Form path is empty or current and requested form do not match, clearing form session {form}, Browser Session:{session.Id}");

                _sessionHelper.Clear();
            }
            
            var sessionGuid = _sessionHelper.GetSessionGuid();
    
            if (string.IsNullOrEmpty(sessionGuid))
            {
                sessionGuid = Guid.NewGuid().ToString();
                _sessionHelper.Set(sessionGuid, form);
                isNewSession = true;    
                _logger.LogInformation($"PageService:ProcessPage: Form SessionID was empty, new form session created for {form}, Browser Session:{session.Id} Form Session: {sessionGuid}");
            }

            var baseForm = await _schemaFactory.Build(form);
            if (baseForm is null)
            {
                _logger.LogWarning($"PageService:ProcessPage: Base form was null, {form}, Browser Session:{session.Id} Form Session: {sessionGuid}");
                _sessionHelper.Clear();
                return null;
            }

            if (!_formAvailabilityService.IsAvailable(baseForm.EnvironmentAvailabilities, _environment.EnvironmentName))
            {
                _logger.LogWarning($"PageService:ProcessPage:Form {form} is not available in environment {_environment.EnvironmentName.ToS3EnvPrefix()}, Browser Session:{session.Id} Form Session: {sessionGuid}");
                _sessionHelper.Clear();
                return null;
            }

            if (isNewSession && !_formAvailabilityService.IsFormAccessApproved(baseForm))
            {
                _logger.LogInformation($"PageService:ProcessPage:Access to {form} was not approved, Browser Session:{session.Id} Form Session: {sessionGuid}");
                _sessionHelper.Clear();
                return null;
            }

            if (string.IsNullOrEmpty(path))
            {
                _logger.LogInformation($"PageService:ProcessPage:Path was empty, redirect to first page of form {form}, Browser Session:{session.Id} Form Session: {sessionGuid}");
                return new ProcessPageEntity { ShouldRedirect = true, TargetPage = baseForm.FirstPageSlug };
            }
                
            var formData = _distributedCache.GetString(sessionGuid);
            if (string.IsNullOrEmpty(formData) && !path.Equals(baseForm.FirstPageSlug) && (!baseForm.HasDocumentUpload || !path.Equals(FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH)))
            {
                _logger.LogInformation($"PageService:ProcessPage:Form data was empty and path was not the first page redirect to first page of form {form}, Browser Session:{session.Id} Form Session: {sessionGuid}");
                return new ProcessPageEntity { ShouldRedirect = true, TargetPage = baseForm.FirstPageSlug };
            }

            if (!string.IsNullOrEmpty(formData) && path.Equals(baseForm.FirstPageSlug))
            {
                var convertedFormData = JsonConvert.DeserializeObject<FormAnswers>(formData);
                if (!form.Equals(convertedFormData.FormName, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning($"PageService:ProcessPage: Disposing session form names do not match {form}, {convertedFormData.FormName},  Browser Session:{session.Id} Form Session: {sessionGuid}");
                    _distributedCache.Remove(sessionGuid);
                }
            }

            var page = baseForm.GetPage(_pageHelper, path);
            if (page is null)
                throw new ApplicationException($"PageService:ProcessPage: Requested path '{path}' object could not be found for in '{form}', Browser Session:{session.Id} Form Session: {sessionGuid}");

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
                _pageHelper.SaveNonQuestionAnswers(result, form, path, sessionGuid);
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
            _logger.LogInformation($"PageService:ProcessPage: Finish processing page \"{form}/{path}/{subPath}\", Browser Session:{session.Id} Form Session: {sessionGuid}"); 
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
            var session = _sessionHelper.GetSession();
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (!_formAvailabilityService.IsAvailable(baseForm.EnvironmentAvailabilities, _environment.EnvironmentName))
                throw new ApplicationException($"PageService:ProcessRequest: {form} is not available in this Environment: {_environment.EnvironmentName.ToS3EnvPrefix()}, Browser Session:{session.Id} Form Session: {sessionGuid}");
            
            if (sessionGuid is null)
                throw new NullReferenceException($"PageService:ProcessRequest: {form} Form Session guid is null, Browser Session:{session.Id} Form Session: {sessionGuid}");

            var currentPage = baseForm.GetPage(_pageHelper, path);
            if (currentPage is null)
                throw new NullReferenceException($"PageService:ProcessRequest: {form} Current page '{path}' object could not be found, Browser Session:{session.Id} Form Session: {sessionGuid}");

            var formData = _distributedCache.GetString(sessionGuid);
            var convertedAnswers = !string.IsNullOrEmpty(formData) ? JsonConvert.DeserializeObject<FormAnswers>(formData) : new FormAnswers();
            await _schemaFactory.TransformPage(currentPage, convertedAnswers);

            if (currentPage.HasIncomingPostValues)
                viewModel = _incomingDataHelper.AddIncomingFormDataValues(currentPage, viewModel);

            viewModel = _pageHelper.SanitizeViewModel(viewModel);
            currentPage.Validate(viewModel, _validators, baseForm);

            if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.AddAnother)))
                return await _addAnotherService.ProcessAddAnother(viewModel, currentPage, baseForm, sessionGuid, path);

            if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Address)))
                return await _addressService.ProcessAddress(viewModel, currentPage, baseForm, sessionGuid, path);

            if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Street)))
                return await _streetService.ProcessStreet(viewModel, currentPage, baseForm, sessionGuid, path);

            if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Organisation)))
                return await _organisationService.ProcessOrganisation(viewModel, currentPage, baseForm, sessionGuid, path);

            if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.MultipleFileUpload)))
                return await _fileUploadService.ProcessFile(viewModel, currentPage, baseForm, sessionGuid, path, files, modelStateIsValid);

            if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Booking)))
                return await _bookingService.ProcessBooking(viewModel, currentPage, baseForm, sessionGuid, path);

            _pageHelper.SaveAnswers(viewModel, sessionGuid, baseForm.BaseURL, files, currentPage.IsValid);

            if (!currentPage.IsValid)
            {
                var formModel = await _pageContentFactory.Build(currentPage, viewModel, baseForm, sessionGuid);
                return new ProcessRequestEntity { Page = currentPage, ViewModel = formModel };
            }

            return new ProcessRequestEntity { Page = currentPage };
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

        public async Task<Behaviour> GetBehaviour(ProcessRequestEntity currentPageResult)
        {
            var answers = new Dictionary<string, dynamic>();
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var cachedAnswers = _distributedCache.GetString(sessionGuid);
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
            var sessionGuid = _sessionHelper.GetSessionGuid();

            var session = _sessionHelper.GetSession();
            if(session is null)
            {
                _logger.LogInformation($"PageService:FinalisePageJourney: Browser session was empty {form}");
            }

            if (string.IsNullOrEmpty(sessionGuid))
                throw new ApplicationException($"PageService::FinalisePageJourney:{session.Id} - {sessionGuid} Session has expired");

            var formData = _distributedCache.GetString(sessionGuid);

            if (formData is null)
                throw new ApplicationException($"PageService::FinalisePageJourney:{session.Id} - {sessionGuid} Session data is null");

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

            if (baseForm.Pages.Where(_ => _.PageSlug.ToLower().Equals("success")).Any() && baseForm.GetPage(_pageHelper, "success").Elements.Where(_ => _.Type.Equals(EElementType.DocumentDownload)).Any())
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