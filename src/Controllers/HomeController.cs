using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using form_builder.Models;
using form_builder.ViewModels;
using Newtonsoft.Json;
using form_builder.Enum;
using form_builder.Validators;
using System.Threading.Tasks;
using System;
using StockportGovUK.AspNetCore.Gateways;
using form_builder.Helpers.PageHelpers;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace form_builder.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDistributedCacheWrapper _distributedCache;

        private readonly IEnumerable<IElementValidator> _validators;

        private readonly ISchemaProvider _schemaProvider;

        private readonly IGateway _gateway;

        private readonly IPageHelper _pageHelper;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IDistributedCacheWrapper distributedCache, IEnumerable<IElementValidator> validators, ISchemaProvider schemaProvider, IGateway gateway, IPageHelper pageHelper)
        {
            _distributedCache = distributedCache;
            _validators = validators;
            _schemaProvider = schemaProvider;
            _gateway = gateway;
            _pageHelper = pageHelper;
            _logger = logger;
        }

        [HttpGet]
        [Route("{form}")]
        [Route("{form}/{path}")]
        public async Task<IActionResult> Index(string form, string path)
        {
            try
            {
                var sessionGuid = HttpContext.Session.GetString("sessionGuid");

                if (sessionGuid == null)
                {
                    sessionGuid = new Guid().ToString();
                    HttpContext.Session.SetString("sessionGuid", sessionGuid);
                }

                var baseForm = await _schemaProvider.Get<FormSchema>(form);

                if (string.IsNullOrEmpty(path))
                {
                    path = baseForm.StartPageSlug;
                }

                var page = baseForm.GetPage(path);
                if (page == null)
                {
                    return RedirectToAction("Error");
                }

                if (page.Elements.Any(_ => _.Type == EElementType.Address))
                {
                    return RedirectToAction("Index", "Address",
                        new
                        {
                            form,
                            path,
                        }
                    );
                }

                var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm, sessionGuid);

                viewModel.Path = path;
                viewModel.FormName = baseForm.FormName;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { ex = ex.Message, form });
            }
        }

        [HttpPost]
        [Route("{form}")]
        [Route("{form}/{path}")]
        public async Task<IActionResult> Index(string form, string path, Dictionary<string, string[]> formData)
        {
            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var currentPage = baseForm.GetPage(path);
            var viewModel = NormaliseFormData(formData);

            var sessionGuid = HttpContext.Session.GetString("sessionGuid");

            if (currentPage == null)
            {
                return RedirectToAction("Error");
            }

            currentPage.Validate(viewModel, _validators);
            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, sessionGuid);
                formModel.Path = currentPage.PageSlug;
                formModel.FormName = baseForm.FormName;
                return View(formModel);
            }

            var behaviour = currentPage.GetNextPage(viewModel);
            _pageHelper.SaveAnswers(viewModel, sessionGuid);

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
                    return RedirectToAction("Submit", "Home", new
                    {
                        form = baseForm.BaseURL
                    });
                default:
                    return RedirectToAction("Error");
            }
        }

        [HttpGet]
        [Route("{form}/submit")]
        public async Task<IActionResult> Submit(string form)
        {
            var sessionGuid = HttpContext.Session.GetString("sessionGuid");

            if (string.IsNullOrEmpty(sessionGuid))
            {
                return RedirectToAction("Error", new { form });
            }

            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var formData = _distributedCache.GetString(sessionGuid);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            convertedAnswers.FormName = form;

            var currentPage = baseForm.GetPage(convertedAnswers.Path);
            var postUrl = currentPage.GetSubmitFormEndpoint(convertedAnswers);

            if (string.IsNullOrEmpty(postUrl))
            {
                _logger.LogError("HomeController, Submit: No postUrl supplied for submit form");
                return RedirectToAction("Error", new { form });
            }

            var postData = CreatePostData(convertedAnswers);

            var reference = string.Empty;

            try
            {
                var response = await _gateway.PostAsync(postUrl, postData);
                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
                    reference = JsonConvert.DeserializeObject<string>(content);
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError($"HomeController, Submit: Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
                    return RedirectToAction("Error", new { form });
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"HomeController, Submit: An exception has occured while attemping to call {postUrl}, Exception Message: {e.Message}");
                return RedirectToAction("Error", new { form });
            }

            _distributedCache.Remove(sessionGuid);
            HttpContext.Session.Remove("sessionGuid");

            var page = baseForm.GetPage("success");
            if(page == null)
            {
                return View("Submit", convertedAnswers);
            }

            var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm, sessionGuid);

            var success = new Success { 
                FormName = baseForm.FormName,
                Reference = reference,
                FormAnswers = convertedAnswers,
                PageContent = viewModel.RawHTML,
                SecondaryHeader = page.Title
            };

            ViewData["BannerTypeformUrl"] = baseForm.FeedbackForm;
            return View("Success", success);
        }

        [HttpGet]
        [Route("{form}/error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string ex)
        {
            ViewData["Error"] = ex;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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

        protected PostData CreatePostData(FormAnswers formAnswers)
        {
            var postData = new PostData() { Form = formAnswers.FormName, Answers= new List<Answers>()};

            if (formAnswers.Pages == null)
                return postData;

            foreach(var page in formAnswers.Pages)
            {
                foreach(var a in page.Answers)
                {
                    postData.Answers.Add(a);
                }
            }

            return postData;

        }
    }
}