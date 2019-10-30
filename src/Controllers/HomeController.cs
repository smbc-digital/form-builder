using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using form_builder.Models;
using form_builder.ViewModels;
using Newtonsoft.Json;
using form_builder.Enum;
using form_builder.Providers;
using form_builder.Validators;
using System.Threading.Tasks;
using form_builder.Helpers;
using System;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Options;
using form_builder.Configuration;
using System.Linq;
using StockportGovUK.AspNetCore.Gateways;

namespace form_builder.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICacheProvider _cacheProvider;

        private readonly IEnumerable<IElementValidator> _validators;

        private readonly IViewRender _viewRender;

        private readonly ISchemaProvider _schemaProvider;

        private readonly DisallowedAnswerKeysConfiguration _disallowedKeys;

        private readonly IGateway _gateway;

        //private readonly IHttpClient _client;

        public HomeController(ICacheProvider cacheProvider, IEnumerable<IElementValidator> validators, ISchemaProvider schemaProvider, IViewRender viewRender, IOptions<DisallowedAnswerKeysConfiguration> disallowedKeys, IGateway gateway)
        {
            _cacheProvider = cacheProvider;
            _validators = validators;
            _schemaProvider = schemaProvider;
            _viewRender = viewRender;
            _disallowedKeys = disallowedKeys.Value;
            _gateway = gateway;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("{form}")]
        [Route("{form}/{path}")]
        public async Task<IActionResult> Index(string form, string path, [FromQuery] Guid guid)
        {
            try
            {
                var baseForm = await _schemaProvider.Get<FormSchema>(form);

                if (Guid.Empty == guid)
                {
                    guid = Guid.NewGuid();
                }

                if (string.IsNullOrEmpty(path))
                {
                    path = baseForm.StartPage;
                }

                var page = baseForm.GetPage(path);
                if (page == null)
                {
                    return RedirectToAction("Error");
                }

                var viewModel = await GenerateHtml(page, new Dictionary<string, string>(), baseForm);

                viewModel.Path = path;
                viewModel.Guid = guid;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        [Route("{form}")]
        [Route("{form}/{path}")]
        public async Task<IActionResult> Index(string form, string path, Dictionary<string, string> viewModel)
        {
            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var currentPage = baseForm.GetPage(path);
            var guid = Guid.Parse(viewModel["Guid"]);

            if (currentPage == null)
            {
                return RedirectToAction("Error");
            }

            currentPage.Validate(viewModel, _validators);
            if (!currentPage.IsValid)
            {
                var formModel = await GenerateHtml(currentPage, viewModel, baseForm);
                formModel.Path = currentPage.PageURL;
                formModel.Guid = guid;
                return View(formModel);
            }

            var behaviour = currentPage.GetNextPage(viewModel);
            SaveAnswers(viewModel);

            switch (behaviour.BehaviourType)
            {
                case EBehaviourType.GoToExternalPage:
                    return Redirect(behaviour.pageURL);
                case EBehaviourType.GoToPage:
                    return RedirectToAction("Index", "Home", new
                    {
                        path = behaviour.pageURL,
                        guid
                    });
                case EBehaviourType.SubmitForm:
                    return RedirectToAction("Submit", "Home", new
                    {
                        guid
                    });
                default:
                    return RedirectToAction("Error");
            }
        }

        [HttpGet]
        [Route("submit")]
        public async Task<IActionResult> Submit([FromQuery] Guid guid)
        {
            var formData = _cacheProvider.GetString(guid.ToString());
            var convertedAnswers = JsonConvert.DeserializeObject<List<FormAnswers>>(formData);

            try
            {
                //_gateway.ChangeAuthenticationHeader("TestToken");
                await _gateway.PostAsync(null, convertedAnswers);
            }
            catch (Exception e)
            {
                throw;
            }

            _cacheProvider.RemoveKey(guid.ToString());
            return View("Submit", convertedAnswers);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void SaveAnswers(Dictionary<string, string> viewModel)
        {
            var guid = viewModel["Guid"];
            var formData = _cacheProvider.GetString(guid);
            var convertedAnswers = new List<FormAnswers>();

            if (!string.IsNullOrEmpty(formData)) {
                convertedAnswers = JsonConvert.DeserializeObject<List<FormAnswers>>(formData);
            }

            if (convertedAnswers.Any(_ => _.PageUrl == viewModel["Path"].ToLower()))
            {
                convertedAnswers = convertedAnswers.Where(_ => _.PageUrl != viewModel["Path"].ToLower())
                                                    .ToList();
            }

            var answers = new List<Answers>();

            foreach (var item in viewModel)
            {
                if (!_disallowedKeys.DisallowedAnswerKeys.Contains(item.Key))
                {
                    answers.Add(new Answers { QuestionId = item.Key, Response = item.Value });
                }
            }

            convertedAnswers.Add(new FormAnswers
            {
                PageUrl = viewModel["Path"].ToLower(),
                Answers = answers
            });
            _cacheProvider.SetString(guid, JsonConvert.SerializeObject(convertedAnswers), 30);
        }

        private async Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, string> viewModel, FormSchema baseForm)
        {
            //To refactor out the switch with a renderasync.element (see SL)
            FormBuilderViewModel formModel = new FormBuilderViewModel();
            formModel.RawHTML += await _viewRender.RenderAsync("H1", baseForm.Name);
            formModel.FeedbackForm = baseForm.FeedbackForm;
            foreach (var element in page.Elements)
            {
                
                switch (element.Type)
                {
                    case EElementType.H1:
                        formModel.RawHTML += await _viewRender.RenderAsync("H1", element.Properties.Text);
                        break;
                    case EElementType.H2:
                        formModel.RawHTML += await _viewRender.RenderAsync("H2", element);
                        break;
                    case EElementType.H3:
                        formModel.RawHTML += await _viewRender.RenderAsync("H3", element);
                        break;
                    case EElementType.P:
                        formModel.RawHTML += await _viewRender.RenderAsync("P", element);
                        break;
                    case EElementType.Textbox:
                        element.Properties.Value = CurrentValue(element, viewModel);
                        CheckForLabel(element, viewModel);
                        formModel.RawHTML += await _viewRender.RenderAsync("Textbox", element);
                        break;
                    case EElementType.Textarea:
                        element.Properties.Value = CurrentValue(element, viewModel);
                        formModel.RawHTML += await _viewRender.RenderAsync("Textarea", element);
                        break;
                    case EElementType.Radio:
                        element.Properties.Value = CurrentValue(element, viewModel);
                        formModel.RawHTML += await _viewRender.RenderAsync("Radio", element);
                        break;
                    case EElementType.Button:
                        formModel.RawHTML += await _viewRender.RenderAsync("Button", element);
                        break;
                    default:
                        break;
                }
            }

            return formModel;
        }

        private string CurrentValue(Element element, Dictionary<string, string> viewModel)
        {
            var currentValue = viewModel.ContainsKey(element.Properties.QuestionId);
            return currentValue ? viewModel[element.Properties.QuestionId] : string.Empty;
        }

        private bool CheckForLabel(Element element, Dictionary<string, string> viewModel)
        {
            //var currentLabel = viewModel.ContainsKey(element.Properties.Label);
            if(string.IsNullOrEmpty(element.Properties.Label))
            {
                throw new Exception("no label");
            }

            return true;
        }
    }
}