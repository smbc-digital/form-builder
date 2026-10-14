namespace form_builder.Services.PageService;

public class PageService(IEnumerable<IElementValidator> validators,
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
    : IPageService
{
    private readonly IFileStorageProvider _fileStorageProvider = fileStorageProviders.Get(fileStorageConfiguration.Value.Type);
    private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;

    public async Task<ProcessPageEntity> ProcessPage(string form, string path, string subPath, IQueryCollection queryParameters)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        
        if (string.IsNullOrEmpty(sessionHelper.GetSessionFormName(form)))
            sessionHelper.SetSessionFormName(form, "started");

        string cacheId = $"{form}::{browserSessionId}";

        logger.LogInformation($"PageService:ProcessPage: Start processing page \"{form}/{path}/{subPath}\", Cache Id: {cacheId}");

        var formData = distributedCache.GetString(cacheId);
        var pathIsEmpty = string.IsNullOrEmpty(path);
        var cacheIsEmpty = formData is null;

        if (pathIsEmpty)
        {
            logger.LogInformation($"PageService:ProcessPage: New Cache created for {cacheId}");
            await distributedCache.SetStringAsync(cacheId, JsonConvert.SerializeObject(new FormAnswers
            {
                Pages = new List<PageAnswers>(),
                AdditionalFormData = queryParameters.ToDictionary<KeyValuePair<string, StringValues>, string, object>(pair => pair.Key, pair => pair.Value.ToString())
            }));
        }

        if (cacheIsEmpty)
        {
            logger.LogInformation($"PageService:ProcessPage: Cache Id was not found in Cache, new Cache created for {cacheId}");
            await distributedCache.SetStringAsync(cacheId, JsonConvert.SerializeObject(new FormAnswers
            {
                Pages = new List<PageAnswers>(),
                AdditionalFormData = queryParameters.ToDictionary<KeyValuePair<string, StringValues>, string, object>(pair => pair.Key, pair => pair.Value.ToString())
            }));
        }

        var baseForm = await schemaFactory.Build(form);
        if (baseForm is null)
        {
            logger.LogWarning($"PageService:ProcessPage: Base form was null, Cache Id: {cacheId}");
            distributedCache.Remove(cacheId);
            return null;
        }

        if (!formAvailabilityService.IsAvailable(baseForm.EnvironmentAvailabilities, environment.EnvironmentName))
        {
            logger.LogWarning($"PageService:ProcessPage:Form {form} is not available in environment {environment.EnvironmentName.ToS3EnvPrefix()}");
            distributedCache.Remove(cacheId);
            return new ProcessPageEntity 
            { 
                TargetPage = "unavailable",
                ViewModel = new FormBuilderViewModel
                {
                    StartPageUrl = baseForm.StartPageUrl,
                    FormName = baseForm.FormName,
                    UnavailableReason = baseForm.EnvironmentAvailabilities.First(env => env.Environment.Equals(environment.EnvironmentName)).UnavailableReason
                }
            };
        }

        if ((pathIsEmpty || cacheIsEmpty) && !formAvailabilityService.IsFormAccessApproved(baseForm))
        {
            logger.LogInformation($"PageService:ProcessPage:Access to {form} was not approved, Cache Id: {cacheId}");
            distributedCache.Remove(cacheId);
            return null;
        }

        if (string.IsNullOrEmpty(path))
        {
            logger.LogInformation($"PageService:ProcessPage:Path was empty, redirect to first page of form, Cache Id: {cacheId}");
            return new ProcessPageEntity { ShouldRedirect = true, TargetPage = baseForm.FirstPageSlug };
        }

        if (string.IsNullOrEmpty(formData) && !path.Equals(baseForm.FirstPageSlug) && (!baseForm.HasDocumentUpload || !path.Equals(FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH)))
        {
            logger.LogInformation($"PageService:ProcessPage:Form data was empty and path was not the first page, redirect to first page of form, Cache Id: {cacheId}");
            return new ProcessPageEntity { ShouldRedirect = true, TargetPage = baseForm.FirstPageSlug };
        }

        if (!string.IsNullOrEmpty(formData) && path.Equals(baseForm.FirstPageSlug))
        {
            var convertedFormData = JsonConvert.DeserializeObject<FormAnswers>(formData);
            if (!string.IsNullOrEmpty(convertedFormData.FormName) && !form.Equals(convertedFormData.FormName, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning($"PageService:ProcessPage: Disposing session as form names do not match {form}, {convertedFormData.FormName},  Cache Id: {cacheId}");
                distributedCache.Remove(cacheId);
            }
        }

        var page = baseForm.GetPage(pageHelper, path, form);
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
                await schemaFactory.TransformPage(schemaPage, convertedAnswers);
        }
        else
        {
            await schemaFactory.TransformPage(page, convertedAnswers);
        }

        if (subPath.Equals(LookUpConstants.Automatic) || subPath.Equals(LookUpConstants.Manual))
        {
            if (convertedAnswers.FormData.ContainsKey($"{path}{LookUpConstants.SearchResultsKeyPostFix}"))
                searchResults = ((IEnumerable<object>)convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"])?.ToList();
        }

        if (page.HasIncomingGetValues)
        {
            var result = incomingDataHelper.AddIncomingFormDataValues(page, queryParameters, convertedAnswers);
            pageHelper.SaveNonQuestionAnswers(result, form, path, cacheId);
            formData = distributedCache.GetString(cacheId);
            convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
        }

        var validCaseRef = convertedAnswers.AdditionalFormData.ContainsKey(ValidateConstants.ValidateId);
        var actions = page.PageActions.Where(_ => _.Properties.HttpActionType.Equals(EHttpActionType.Get)).ToList();

        if (page.HasPageActionsGetValues)
        {
            if (actions.Any(_ => _.Type.Equals(EActionType.Validate)) && !validCaseRef ||
                !actions.Any(_ => _.Type.Equals(EActionType.Validate)))
                await actionsWorkflow.Process(page.PageActions.Where(_ => _.Properties.HttpActionType.Equals(EHttpActionType.Get)).ToList(), null, form);

            formData = distributedCache.GetString(cacheId);
            convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
        }

        if (page.Elements.Any(_ => _.Type.Equals(EElementType.Booking)))
        {
            foreach (var tagParser in tagParsers)
            {
                await tagParser.Parse(page, convertedAnswers, baseForm);
            }

            var bookingProcessEntity = await bookingService.Get(baseForm.BaseURL, page, cacheId);

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

        var viewModel = await GetViewModel(page, baseForm, path, cacheId, subPath, searchResults, convertedAnswers);
        logger.LogInformation($"PageService:ProcessPage: Finish processing page \"{form}/{path}/{subPath}\", Cache Id: {cacheId}"); 
        return new ProcessPageEntity { ViewModel = viewModel };
    }

    public async Task<ProcessRequestEntity> ProcessRequest(
        string form,
        string path,
        Dictionary<string, dynamic> viewModel,
        IEnumerable<CustomFormFile> files,
        bool modelStateIsValid)
    {
        FormSchema baseForm = await schemaFactory.Build(form);
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string cacheId = $"{form}::{browserSessionId}";

        if (!formAvailabilityService.IsAvailable(baseForm.EnvironmentAvailabilities, environment.EnvironmentName))
            throw new ApplicationException($"PageService:ProcessRequest: {form} is not available in this Environment: {environment.EnvironmentName.ToS3EnvPrefix()}, Cache Id: {cacheId}");

        if (browserSessionId is null)
            throw new NullReferenceException($"PageService:ProcessRequest: {form} Browser Session is null");

        var currentPage = baseForm.GetPage(pageHelper, path, form);
        if (currentPage is null)
            throw new NullReferenceException($"PageService:ProcessRequest: {form} Current page '{path}' object could not be found, Cache Id: {cacheId}");

        var formData = distributedCache.GetString(cacheId);
        var convertedAnswers = !string.IsNullOrEmpty(formData) ? JsonConvert.DeserializeObject<FormAnswers>(formData) : new FormAnswers { Pages = new List<PageAnswers>() };
        await schemaFactory.TransformPage(currentPage, convertedAnswers);

        if (currentPage.HasIncomingPostValues)
            viewModel = incomingDataHelper.AddIncomingFormDataValues(currentPage, viewModel);

        viewModel = pageHelper.SanitizeViewModel(viewModel);
        currentPage.Validate(viewModel, validators, baseForm);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.AddAnother)))
            return await addAnotherService.ProcessAddAnother(viewModel, currentPage, baseForm, cacheId, path);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Address)))
            return await addressService.ProcessAddress(viewModel, currentPage, baseForm, cacheId, path);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Street)))
            return await streetService.ProcessStreet(viewModel, currentPage, baseForm, cacheId, path);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Organisation)))
            return await organisationService.ProcessOrganisation(viewModel, currentPage, baseForm, cacheId, path);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.MultipleFileUpload)))
            return await fileUploadService.ProcessFile(viewModel, currentPage, baseForm, cacheId, path, files, modelStateIsValid);

        if (currentPage.Elements.Any(_ => _.Type.Equals(EElementType.Booking)))
            return await bookingService.ProcessBooking(viewModel, currentPage, baseForm, cacheId, path);

        pageHelper.SaveAnswers(viewModel, cacheId, baseForm.BaseURL, files, currentPage.IsValid);

        if (!currentPage.IsValid)
        {
            var formModel = await pageFactory.Build(currentPage, viewModel, baseForm, cacheId);
            return new ProcessRequestEntity { Page = currentPage, ViewModel = formModel };
        }

        return new ProcessRequestEntity { Page = currentPage };
    }

    public async Task<FormBuilderViewModel> GetViewModel(Page page, FormSchema baseForm, string path, string cacheKey, string subPath, List<object> results, FormAnswers convertedAnswers)
    {
        var viewModelData = new Dictionary<string, dynamic>
        {
            { LookUpConstants.SubPathViewModelKey, subPath }
        };

        var viewModel = await pageFactory.Build(page, viewModelData, baseForm, cacheKey, convertedAnswers, results);

        return viewModel;
    }

    public async Task<Behaviour> GetBehaviour(ProcessRequestEntity currentPageResult, string form)
    {
        var answers = new Dictionary<string, dynamic>();
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string formSessionId = $"{form}::{browserSessionId}";
        var cachedAnswers = distributedCache.GetString(formSessionId);
        var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        convertedAnswers.Pages
            .SelectMany(_ => _.Answers)
            .ToList()
            .ForEach(x => answers.Add(x.QuestionId, x.Response));

        foreach (var tagParser in tagParsers)
        {
            await tagParser.Parse(currentPageResult.Page, convertedAnswers, null);
        }

        if (answers.Count > 0)
        {
            Dictionary<string, object> newFormData = new();
            foreach (var answer in convertedAnswers.AdditionalFormData)
            {
                if (!answers.ContainsKey(answer.Key))
                    newFormData.Add(answer.Key, answer.Value);
            }

            answers.AddRange(newFormData);
        }
        else
        {
            answers.AddRange(convertedAnswers.AdditionalFormData);
        }

        return currentPageResult.Page.GetNextPage(answers);
    }

    public async Task<SuccessPageEntity> FinalisePageJourney(string form, EBehaviourType behaviourType, FormSchema baseForm)
    {
        logger.LogInformation($"PageService:FinalisePageJourney: finalising success page journey for {form} with behaviour type {behaviourType}");

        string browserSessionId = sessionHelper.GetBrowserSessionId();

        if (string.IsNullOrEmpty(browserSessionId))
            throw new ApplicationException($"PageService::FinalisePageJourney:{form} - Browser Session is null for {form} with behaviour type {behaviourType}");

        string formSessionId = $"{form}::{browserSessionId}";

        var formData = distributedCache.GetString(formSessionId);

        if (formData is null)
            throw new ApplicationException($"PageService::FinalisePageJourney: {formSessionId} Session data is null for {form} with behaviour type {behaviourType}");

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

        if (baseForm.Pages.Where(_ => _.PageSlug.ToLower().Equals("success")).Any() && baseForm.GetPage(pageHelper, "success", form).Elements.Where(_ => _.Type.Equals(EElementType.DocumentDownload)).Any())
            await distributedCache.SetStringAsync($"document-{formSessionId}", JsonConvert.SerializeObject(formAnswers), _distributedCacheExpirationConfiguration.Document);

        return await successPageFactory.Build(form, baseForm, formSessionId, formAnswers, behaviourType);
    }

    public async Task<SuccessPageEntity> GetCancelBookingSuccessPage(string form)
    {
        var baseForm = await schemaFactory.Build(form);
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string formSessionId = $"{form}::{browserSessionId}";
        return await successPageFactory.BuildBooking(form, baseForm, formSessionId, new FormAnswers());
    }
}