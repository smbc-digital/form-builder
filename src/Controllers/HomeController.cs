using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using form_builder.Enum;
using System.Threading.Tasks;
using System;
using form_builder.Services.PageService;
using form_builder.Services.SubmtiService;
using form_builder.Extensions;
using form_builder.Helpers.Session;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json.Linq;
using form_builder.Models;

namespace form_builder.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPageService _pageService;
        private readonly ISubmitService _submitService;

        public HomeController(IPageService pageService, ISubmitService submitService)
        {
            _pageService = pageService;
            _submitService = submitService;
        }

        [HttpGet]
        [Route("{form}")]
        [Route("{form}/{path}")]
        public async Task<IActionResult> Index(string form, string path)
        {
            var response = await _pageService.ProcessPage(form, path);
            if (response.ShouldRedirect)
            {
                return RedirectToAction("Index", new
                {
                    path = response.TargetPage,
                    form
                });
            }

            return View(response.ViewName, response.ViewModel);
        }

        [HttpGet]
        [Route("{form}/{path}/manual")]
        public async Task<IActionResult> AddressManual(string form, string path)
        {
            var response = await _pageService.ProcessPage(form, path, true);
            if (response.ShouldRedirect)
            {
                return RedirectToAction("Index", new
                {
                    path = response.TargetPage,
                    form
                });
            }

            return View(response.ViewName, response.ViewModel);
        }

        [HttpPost]
        [Route("{form}")]
        [Route("{form}/{path}")]
        public async Task<IActionResult> Index(string form, string path, Dictionary<string, string[]> formData)
        {
            var viewModel = formData.ToNormaliseDictionary();
            var currentPageResult = await _pageService.ProcessRequest(form, path, viewModel);

            if (!currentPageResult.Page.IsValid || currentPageResult.UseGeneratedViewModel)
            {
                return View(currentPageResult.ViewName, currentPageResult.ViewModel);
            }

            Behaviour behaviour;

            if (viewModel == null)
            {
                behaviour = _pageService.GetBehaviour(currentPageResult);
            }
            else
            {
                behaviour = currentPageResult.Page.GetNextPage(viewModel);
            }

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
            var viewModel = formData.ToNormaliseDictionary();
            var currentPageResult = await _pageService.ProcessRequest(form, path, viewModel, true);

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
    }
}