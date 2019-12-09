using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using form_builder.Models;
using form_builder.Enum;
using System.Threading.Tasks;
using System;
using form_builder.Helpers.PageHelpers;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Logging;
using System.Linq;
using form_builder.Helpers.Session;
using form_builder.Services.PageService;
using form_builder.Services.SubmtiService;
using form_builder.Models.Elements;

namespace form_builder.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDistributedCacheWrapper _distributedCache;

        private readonly ISchemaProvider _schemaProvider;

        private readonly IPageHelper _pageHelper;

        private readonly ISessionHelper _sessionHelper;

        private readonly IPageService _pageService;

        private readonly ISubmitService _submitService;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IDistributedCacheWrapper distributedCache, ISchemaProvider schemaProvider, IPageHelper pageHelper, ISessionHelper sessionHelper, IPageService pageService, ISubmitService submitService)
        {
            _distributedCache = distributedCache;
            _schemaProvider = schemaProvider;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _logger = logger;
            _pageService = pageService;
            _submitService = submitService;
        }

        [HttpGet]
        [Route("{form}")]
        [Route("{form}/{path}")]
        public async Task<IActionResult> Index(string form, string path)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
            {
                sessionGuid = Guid.NewGuid().ToString();
                _sessionHelper.SetSessionGuid(sessionGuid);
            }

            var baseForm = await _schemaProvider.Get<FormSchema>(form);

            var formData = _distributedCache.GetString(sessionGuid);

            if (_pageHelper.hasDuplicateQuestionIDs(baseForm.Pages))
            {
                throw new ApplicationException($"The provided json '{baseForm.FormName}' has duplicate QuestionIDs");
            }

            if (formData == null && path != baseForm.StartPageSlug)
            {
                return RedirectToAction("Index", new
                {
                    path = baseForm.StartPageSlug,
                    form
                });
            }

            if (string.IsNullOrEmpty(path))
            {
                return RedirectToAction("Index", new
                {
                    path = baseForm.StartPageSlug,
                    form
                });
            }

            var page = baseForm.GetPage(path);
            if (page == null)
            {
                throw new NullReferenceException($"Requested path '{path}' object could not be found.");
            }

            var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm, sessionGuid);
            viewModel.FormName = baseForm.FormName;
            viewModel.Path = path;

            if (page.Elements.Any(_ => _.Type == EElementType.Street))
            {
                viewModel.StreetStatus = "Search";
                return View("../Street/Index", viewModel);
            }

            if (page.Elements.Any(_ => _.Type == EElementType.Address))
            {
                viewModel.AddressStatus = "Search";
                return View("../Address/Index", viewModel);
            }

            return View(viewModel);
        }

        [HttpGet]
        [Route("{form}/{path}/manual")]
        public async Task<IActionResult> AddressManual(string form, string path)
        {
            try
            {
                var sessionGuid = _sessionHelper.GetSessionGuid();

                if (sessionGuid == null)
                {
                    sessionGuid = Guid.NewGuid().ToString();
                    _sessionHelper.SetSessionGuid(sessionGuid);
                }

                var baseForm = await _schemaProvider.Get<FormSchema>(form);

                if (string.IsNullOrEmpty(path))
                {
                    path = baseForm.StartPageSlug;
                }

                var page = baseForm.GetPage(path);
                if (page == null)
                {
                    throw new ApplicationException($"AddressController: GetPage returned null for path: {path} of form: {form}, while performing Get");
                }

                var addressManualElememt = new AddressManual() { Properties = page.Elements[0].Properties, Type = EElementType.AddressManual };

                page.Elements[0] = addressManualElememt;
                var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm, sessionGuid);
                viewModel.AddressStatus = "Search";
                viewModel.FormName = baseForm.FormName;

                return View("../Address/Index", viewModel);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"AddressController: An exception has occured while attempting to return Address view Exception: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("{form}")]
        [Route("{form}/{path}")]
        public async Task<IActionResult> Index(string form, string path, Dictionary<string, string[]> formData)
        {
            var viewModel = NormaliseFormData(formData);
            var currentPageResult = await _pageService.ProcessPage(form, path, viewModel);

            if (!currentPageResult.Page.IsValid || currentPageResult.UseGeneratedViewModel)
            {
                return View(currentPageResult.ViewName, currentPageResult.ViewModel);
            }

            var behaviour = currentPageResult.Page.GetNextPage(viewModel);
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
                default:
                    throw new ApplicationException($"The provided behaviour type '{behaviour.BehaviourType}' is not valid");
            }
        }

        [HttpPost]
        [Route("{form}/{path}/manual")]
        public async Task<IActionResult> AddressManual(string form, string path, Dictionary<string, string[]> formData)
        {
            var viewModel = NormaliseFormData(formData);
            var currentPageResult = await _pageService.ProcessPage(form, path, viewModel, true);

            var behaviour = currentPageResult.Page.GetNextPage(viewModel);

            if (!currentPageResult.Page.IsValid || currentPageResult.UseGeneratedViewModel)
            {
                return View(currentPageResult.ViewName, currentPageResult.ViewModel);
            }

            switch (behaviour.BehaviourType)
            {
                case EBehaviourType.GoToExternalPage:
                    return Redirect(behaviour.PageSlug);
                case EBehaviourType.GoToPage:
                    return RedirectToAction("Index", new
                    {
                        path = behaviour.PageSlug,
                        form
                    });
                case EBehaviourType.SubmitForm:
                    return RedirectToAction("Submit", new
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
            var result = await _submitService.ProcessSubmission(form);

            ViewData["BannerTypeformUrl"] = result.FeedbackFormUrl;
            return View(result.ViewName, result.ViewModel);
        }

        protected Dictionary<string, string> NormaliseFormData(Dictionary<string, string[]> formData)
        {

            var normaisedFormData = new Dictionary<string, string>();

            foreach (var item in formData)
            {
                if (item.Value.Length == 1)
                {
                    normaisedFormData.Add(item.Key, item.Value[0]);
                }
                else
                {
                    normaisedFormData.Add(item.Key, string.Join(", ", item.Value));
                }
            }

            return normaisedFormData;
        }
    }
}