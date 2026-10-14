namespace form_builder.Controllers.Home;

public class HomeController(IPageService pageService,
    ISchemaProvider schemaProvider,
    ISchemaFactory schemaFactory,
    ISubmitWorkflow submitWorkflow,
    IPaymentWorkflow paymentWorkflow,
    IRedirectWorkflow redirectWorkflow,
    IFileUploadService fileUploadService,
    IActionsWorkflow actionsWorkflow,
    IEmailWorkflow emailWorkFlow,
    ISuccessWorkflow successWorkflow,
    IStructureMapper structureMapper,
    IOptions<DataStructureConfiguration> dataStructureConfiguration,
    IOptions<QAFormAccessTokenConfiguration> qaFormAccessToken)
    : Controller
{
    private readonly DataStructureConfiguration _dataStructureConfiguration = dataStructureConfiguration.Value;
    private readonly QAFormAccessTokenConfiguration _qaFormAccessToken = qaFormAccessToken.Value;

    [HttpGet]
    [Route("/")]
    [FeatureGate("HomePageFormListings")]
    public async Task<IActionResult> Home()
    {
        var forms = await schemaProvider.IndexSchema();

        var viewModel = new HomeViewModel
        {
            Forms = forms,
            Embeddable = false,
            StartPageUrl = "/",
            FormName = "",
            HideBackButton = true,
            QAFormAccessToken = _qaFormAccessToken.AccessKey
        };

        return View("Home", viewModel);
    }

    [HttpGet]
    [Route("view/{form}")]
    [FeatureGate("HomePageFormListings")]
    public async Task<IActionResult> FormView(string form)
    {
        var schema = await schemaFactory.Build(form);
        var incomingValues = schema.Pages.First().IncomingValues;

        if (incomingValues.Any())
        {
            if (Request.Query.ContainsKey("key") && string.IsNullOrEmpty(schema.FormAccessKeyName))
                return Redirect($"/view/{form}");

            var viewModel = new HomeViewModel
            {
                Embeddable = false,
                StartPageUrl = "/",
                FormName = form,
                HideBackButton = true,
                IncomingValues = incomingValues,
            };

            return View("IncomingValues", viewModel);
        }

        if (!string.IsNullOrEmpty(schema.FormAccessKeyName))
        {
            Dictionary<string, object> queryParams = Request.Query.ToDictionary<KeyValuePair<string, StringValues>, string, object>(pair => pair.Key, pair => pair.Value.ToString());
            return Redirect($"/{form}?key={queryParams["key"]}");
        }
        
        return Redirect($"/{form}");
    }

    [HttpGet]
    [Route("{form}")]
    [Route("{form}/{path}")]
    [Route("{form}/{path}/{subPath}")]
    public async Task<IActionResult> Index(
        string form,
        string path,
        string subPath = "")
    {
        var queryParameters = Request.Query;
        var response = await pageService.ProcessPage(form, path, subPath, queryParameters);

        if (response is null)
            return RedirectToAction("NotFound", "Error");

        if (response.TargetPage.Equals("unavailable"))
            return View("Unavailable", response.ViewModel);

        if (response.ShouldRedirect)
        {
            var routeValuesDictionary = new RouteValueDictionaryBuilder()
                .WithValue("path", response.TargetPage)
                .WithValue("form", form)
                .WithQueryValues(queryParameters)
                .Build();

            return RedirectToAction("Index", routeValuesDictionary);
        }

        return View(response.ViewName, response.ViewModel);
    }

    [HttpPost]
    [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
    [Route("{form}")]
    [Route("{form}/{path}")]
    [Route("{form}/{path}/{subPath}")]
    public async Task<IActionResult> Index(
        string form,
        string path,
        Dictionary<string, string[]> formData,
        IEnumerable<CustomFormFile> fileUpload,
        string subPath = "")
    {
        var viewModel = formData.ToNormaliseDictionary(subPath);

        if (fileUpload is not null && fileUpload.Any())
            viewModel = fileUploadService.AddFiles(viewModel, fileUpload);

        var currentPageResult = await pageService.ProcessRequest(form, path, viewModel, fileUpload, ModelState.IsValid);

        if (currentPageResult.RedirectToAction && !string.IsNullOrWhiteSpace(currentPageResult.RedirectAction))
        {
            return RedirectToAction(currentPageResult.RedirectAction, currentPageResult.RouteValues ?? new { form, path });
        }

        if (!currentPageResult.Page.IsValid || currentPageResult.UseGeneratedViewModel)
        {
            if (!currentPageResult.Page.IsValid)
                currentPageResult.ViewModel.PageTitle = currentPageResult.ViewModel.PageTitle.ToStringWithPrefix(PageTitleConstants.VALIDATION_ERROR_PREFIX);

            return View(currentPageResult.ViewName, currentPageResult.ViewModel);
        }

        if (currentPageResult.Page.HasPageActionsPostValues)
            await actionsWorkflow.Process(currentPageResult.Page.PageActions.Where(_ => _.Properties.HttpActionType.Equals(EHttpActionType.Post)).ToList(), null, form);

        var behaviour = await pageService.GetBehaviour(currentPageResult, form);

        switch (behaviour.BehaviourType)
        {
            case EBehaviourType.GoToExternalPage:
                return Redirect(behaviour.PageSlug);

            case EBehaviourType.GoToPage:
                return RedirectToAction("Index", new
                {
                    path = behaviour.PageSlug
                });

            case EBehaviourType.SubmitForm:
                return RedirectToAction("Submit", new
                {
                    form
                });

            case EBehaviourType.SubmitWithoutSubmission:
                return RedirectToAction("SubmitWithoutSubmission", new
                {
                    form
                });

            case EBehaviourType.SubmitAndPay:
                var result = await paymentWorkflow.Submit(form, path);
                return Redirect(result);

            case EBehaviourType.SubmitAndRedirect:
                var redirectUrl = await redirectWorkflow.Submit(form, path);
                return Redirect(redirectUrl);

            case EBehaviourType.SubmitAndEmail:
                return RedirectToAction("Email", new
                {
                    form
                });

            default:
                throw new ApplicationException($"The provided behaviour type '{behaviour.BehaviourType}' is not valid");
        }
    }

    [HttpGet]
    [Route("{form}/submit")]
    public async Task<IActionResult> Submit(string form)
    {
        await submitWorkflow.Submit(form);

        return RedirectToAction("Success", new
        {
            form
        });
    }

    [HttpGet]
    [Route("{form}/submit-without-submission")]
    public async Task<IActionResult> SubmitWithoutSubmission(string form)
    {
        await submitWorkflow.SubmitWithoutSubmission(form);

        return RedirectToAction("Success", new
        {
            form
        });
    }

    [HttpGet]
    [Route("{form}/email")]
    public async Task<IActionResult> Email(string form)
    {
        await emailWorkFlow.Submit(form);

        return RedirectToAction("Success", new
        {
            form
        });
    }

    [HttpGet]
    [Route("{form}/success")]
    public async Task<IActionResult> Success(string form)
    {
        var result = await successWorkflow.Process(EBehaviourType.SubmitForm, form);

        var success = new SuccessViewModel
        {
            Reference = result.CaseReference,
            PageContent = result.HtmlContent,
            FormAnswers = result.FormAnswers,
            FormName = result.FormName,
            StartPageUrl = result.StartPageUrl,
            Embeddable = result.Embeddable,
            FeedbackPhase = result.FeedbackPhase,
            FeedbackForm = result.FeedbackFormUrl,
            PageTitle = result.PageTitle,
            BannerTitle = result.BannerTitle,
            LeadingParagraph = result.LeadingParagraph,
            DisplayBreadcrumbs = result.DisplayBreadcrumbs,
            Breadcrumbs = result.Breadcrumbs,
            IsInPreviewMode = result.IsInPreviewMode
        };

        return View(result.ViewName, success);
    }

    [HttpGet]
    [Route("{form}/data-structure")]
    public async Task<IActionResult> DataStructure(string form)
    {
        if (!_dataStructureConfiguration.IsEnabled)
            return RedirectToAction("Index", new { form });

        object dataStructure = await structureMapper.CreateBaseFormDataStructure(form);

        var viewModel = new DataStructureViewModel
        {
            FormName = form,
            StartPageUrl = form,
            FeedbackPhase = string.Empty,
            FeedbackForm = string.Empty,
            PageTitle = form,
            Embeddable = false,
            DisplayBreadcrumbs = false,
            Breadcrumbs = null,
            IsInPreviewMode = false,
            DataStructure = dataStructure,
            ObjectExamples = new List<LanguageObjectExample> { new LanguageObjectExample { LanguageName = "C#", ObjectCode = CSharpObjectGenerator.Generate(dataStructure, form) } }
        };

        return View("DataStructure", viewModel);
    }
}