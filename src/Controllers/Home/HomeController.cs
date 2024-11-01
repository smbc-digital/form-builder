using form_builder.Attributes;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.Session;
using form_builder.Mappers.Structure;
using form_builder.Models;
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
using Newtonsoft.Json;

namespace form_builder.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPageService _pageService;
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

        public HomeController(IPageService pageService,
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
            ILogger<HomeController> logger, ISessionHelper sessionHelper)
        {
            _pageService = pageService;
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
        }

        [HttpGet]
        [Route("/")]
        public IActionResult Home()
        {
            if (_hostingEnvironment.EnvironmentName.Equals("prod", StringComparison.OrdinalIgnoreCase))
                return Redirect("https://www.stockport.gov.uk");

            return RedirectToAction("Index", "Error");
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
            var session = _sessionHelper.GetSession();
            var sessionGuid = _sessionHelper.GetBrowserSessionId();

            var queryParameters = Request.Query;

            _logger.LogInformation($"HomeController:Index:Get: Processing page - {form}/{path}/{subPath}, {JsonConvert.SerializeObject(queryParameters)}, Browser Session:{session.Id}, Form Session: {sessionGuid}");
            var response = await _pageService.ProcessPage(form, path, subPath, queryParameters);

            if (response is null)
            {
                _logger.LogInformation($"HomeController:Index:Get: Form or path not found - {form}/{path}/{subPath}, {JsonConvert.SerializeObject(queryParameters)}, Browser Session:{session.Id}, Form Session: {sessionGuid}");
                return RedirectToAction("NotFound", "Error");
            }

            if (response.ShouldRedirect)
            {
                var routeValuesDictionary = new RouteValueDictionaryBuilder()
                    .WithValue("path", response.TargetPage)
                    .WithValue("form", form)
                    .WithQueryValues(queryParameters)
                    .Build();

                _logger.LogInformation($"HomeController:Index:Get: Redirecting to page - {form}/{response.TargetPage}, {JsonConvert.SerializeObject(queryParameters)}, Browser Session:{session.Id}, Form Session: {sessionGuid}");

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
            var session = _sessionHelper.GetSession();
            var sessionGuid = _sessionHelper.GetBrowserSessionId();

            _logger.LogInformation($"HomeController:Index:Post: Processing request - {form}/{path}/{subPath}, Browser Session:{session.Id}, Form Session: {sessionGuid}");

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
                    _logger.LogInformation($"{nameof(HomeController)}::{nameof(Index)}: POST RedirectToAction path = {behaviour.PageSlug},  Browser Session:{session.Id}, Form Session: {sessionGuid}");
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
                    throw new ApplicationException($"The provided behaviour type '{behaviour.BehaviourType}' is not valid, Browser Session:{session.Id}, Form Session: {sessionGuid}");
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
}
