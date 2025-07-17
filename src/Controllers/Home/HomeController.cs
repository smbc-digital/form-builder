﻿using System.Runtime.CompilerServices;
using System.Xml.Schema;
using form_builder.Attributes;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.Session;
using form_builder.Mappers.Structure;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Services.FileUploadService;
using form_builder.Services.PageService;
using form_builder.ViewModels;
using form_builder.Workflows.ActionsWorkflow;
using form_builder.Workflows.EmailWorkflow;
using form_builder.Workflows.PaymentWorkflow;
using form_builder.Workflows.RedirectWorkflow;
using form_builder.Workflows.SubmitWorkflow;
using form_builder.Workflows.SuccessWorkflow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;

namespace form_builder.Controllers.Home;

public class HomeController : Controller
{
    private readonly IPageService _pageService;
    private readonly ISchemaProvider _schemaProvider;
    private readonly ISchemaFactory _schemaFactory;
    private readonly ISubmitWorkflow _submitWorkflow;
    private readonly IEmailWorkflow _emailWorkFlow;
    private readonly IRedirectWorkflow _redirectWorkflow;
    private readonly IPaymentWorkflow _paymentWorkflow;
    private readonly IActionsWorkflow _actionsWorkflow;
    private readonly ISuccessWorkflow _successWorkflow;
    private readonly IFileUploadService _fileUploadService;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IStructureMapper _structureMapper;
    private readonly DataStructureConfiguration _dataStructureConfiguration;
    private readonly ILogger<HomeController> _logger;
    private readonly ISessionHelper _sessionHelper;
    private readonly IFeatureManager _featureManager;

    public HomeController(IPageService pageService,
        ISchemaProvider schemaProvider,
        ISchemaFactory schemaFactory,
        ISubmitWorkflow submitWorkflow,
        IPaymentWorkflow paymentWorkflow,
        IRedirectWorkflow redirectWorkflow,
        IFileUploadService fileUploadService,
        IWebHostEnvironment hostingEnvironment,
        IActionsWorkflow actionsWorkflow,
        IEmailWorkflow emailWorkFlow,
        ISuccessWorkflow successWorkflow,
        IStructureMapper structureMapper,
        IOptions<DataStructureConfiguration> dataStructureConfiguration,
        ILogger<HomeController> logger,
        ISessionHelper sessionHelper,
        IFeatureManager featureManager)
    {
        _pageService = pageService;
        _schemaProvider = schemaProvider;
        _schemaFactory = schemaFactory;
        _submitWorkflow = submitWorkflow;
        _redirectWorkflow = redirectWorkflow;
        _paymentWorkflow = paymentWorkflow;
        _fileUploadService = fileUploadService;
        _hostingEnvironment = hostingEnvironment;
        _actionsWorkflow = actionsWorkflow;
        _successWorkflow = successWorkflow;
        _structureMapper = structureMapper;
        _logger = logger;
        _dataStructureConfiguration = dataStructureConfiguration.Value;
        _emailWorkFlow = emailWorkFlow;
        _sessionHelper = sessionHelper;
        _featureManager = featureManager;
    }

    [HttpGet]
    [Route("/")]
    [FeatureGate("HomePageFormListings")]
    public async Task<IActionResult> Home()
    {
        var forms = await _schemaProvider.IndexSchema();
        var viewModel = new HomeViewModel
        {
            Forms = forms,
            Embeddable = false,
            StartPageUrl = "/",
            FormName = "",
            HideBackButton = true
        };

        return View("Home", viewModel);
    }

    [HttpGet]
    [Route("view/{form}")]
    [FeatureGate("HomePageFormListings")]
    public async Task<IActionResult> View(string form)
    {
        var schema = await _schemaFactory.Build(form);
        var incomingValues = schema.Pages.First().IncomingValues;

        if (incomingValues.Any())
        {
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
        var response = await _pageService.ProcessPage(form, path, subPath, queryParameters);

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
            viewModel = _fileUploadService.AddFiles(viewModel, fileUpload);

        var currentPageResult = await _pageService.ProcessRequest(form, path, viewModel, fileUpload, ModelState.IsValid);

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
            await _actionsWorkflow.Process(currentPageResult.Page.PageActions.Where(_ => _.Properties.HttpActionType.Equals(EHttpActionType.Post)).ToList(), null, form);

        var behaviour = await _pageService.GetBehaviour(currentPageResult, form);

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
                var result = await _paymentWorkflow.Submit(form, path);
                return Redirect(result);

            case EBehaviourType.SubmitAndRedirect:
                var redirectUrl = await _redirectWorkflow.Submit(form, path);
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
        await _submitWorkflow.Submit(form);

        return RedirectToAction("Success", new
        {
            form
        });
    }

    [HttpGet]
    [Route("{form}/submit-without-submission")]
    public async Task<IActionResult> SubmitWithoutSubmission(string form)
    {
        await _submitWorkflow.SubmitWithoutSubmission(form);

        return RedirectToAction("Success", new
        {
            form
        });
    }

    [HttpGet]
    [Route("{form}/email")]
    public async Task<IActionResult> Email(string form)
    {
        await _emailWorkFlow.Submit(form);

        return RedirectToAction("Success", new
        {
            form
        });
    }

    [HttpGet]
    [Route("{form}/success")]
    public async Task<IActionResult> Success(string form)
    {
        var result = await _successWorkflow.Process(EBehaviourType.SubmitForm, form);

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

        object dataStructure = await _structureMapper.CreateBaseFormDataStructure(form);
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
            DataStructure = dataStructure
        };

        return View("DataStructure", viewModel);
    }
}