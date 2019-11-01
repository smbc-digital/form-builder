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
using System;
using StockportGovUK.AspNetCore.Gateways;
using form_builder.Helpers.PageHelpers;

namespace form_builder.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICacheProvider _cacheProvider;

        private readonly IEnumerable<IElementValidator> _validators;

        private readonly ISchemaProvider _schemaProvider;

        private readonly IGateway _gateway;

        private readonly IPageHelper _pageHelper;

        public HomeController(ICacheProvider cacheProvider, IEnumerable<IElementValidator> validators, ISchemaProvider schemaProvider, IGateway gateway, IPageHelper pageHelper)
        {
            _cacheProvider = cacheProvider;
            _validators = validators;
            _schemaProvider = schemaProvider;
            _gateway = gateway;
            _pageHelper = pageHelper;
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

                var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm);

                viewModel.Path = path;
                viewModel.Guid = guid;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { ex = ex.Message });
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
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm);
                formModel.Path = currentPage.PageURL;
                formModel.Guid = guid;
                return View(formModel);
            }

            var behaviour = currentPage.GetNextPage(viewModel);
            _pageHelper.SaveAnswers(viewModel);

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
        public IActionResult Error(string ex)
        {
            ViewData["Error"] = ex;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}