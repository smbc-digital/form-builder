using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using form_builder.Enum;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using form_builder.Services.PageService;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Services.FileUploadService;
using form_builder.Workflows;
using Microsoft.EntityFrameworkCore.Internal;

namespace form_builder.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPageService _pageService;
        private readonly ISubmitWorkflow _submitWorkflow;
        private readonly IPaymentWorkflow _paymentWorkflow;
        private readonly IFileHelper _fileHelper;
        private readonly IFileUploadService _fileUploadService;

        public HomeController(IPageService pageService, ISubmitWorkflow submitWorkflow, IPaymentWorkflow paymentWorkflow, IFileHelper fileHelper, IFileUploadService fileUploadService)
        {
            _pageService = pageService;
            _submitWorkflow = submitWorkflow;
            _paymentWorkflow = paymentWorkflow; 
            _fileHelper = fileHelper;
            _fileUploadService = fileUploadService;
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
        public async Task<IActionResult> Index(string form, string path, Dictionary<string, string[]> formData, IFormFileCollection fileUpload)
        {
            var viewModel = formData.ToNormaliseDictionary();

            if(fileUpload != null)
            {
                if (fileUpload.Any())
                {
                    viewModel = _fileUploadService.AddFiles(viewModel, fileUpload, form, path);
                }
            }

            var currentPageResult = await _pageService.ProcessRequest(form, path, viewModel, fileUpload);

            if(currentPageResult.RedirectToAction && !string.IsNullOrWhiteSpace(currentPageResult.RedirectAction)){
                return RedirectToAction(currentPageResult.RedirectAction, new
                    {
                        form,
                        path
                    });
            }

            if (!currentPageResult.Page.IsValid || currentPageResult.UseGeneratedViewModel)
            {
                return View(currentPageResult.ViewName, currentPageResult.ViewModel);
            }

            var behaviour = _pageService.GetBehaviour(currentPageResult);

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
                case EBehaviourType.SubmitAndPay:
                    var result = await _paymentWorkflow.Submit(form, path);
                    return Redirect(result);
                default:
                    throw new ApplicationException($"The provided behaviour type '{behaviour.BehaviourType}' is not valid");
            }
        }

        [HttpPost]
        [Route("{form}/{path}/manual")]
        public async Task<IActionResult> AddressManual(string form, string path, Dictionary<string, string[]> formData)
        {
            var viewModel = formData.ToNormaliseDictionary();
            var currentPageResult = await _pageService.ProcessRequest(form, path, viewModel, null, true);

            if (!currentPageResult.Page.IsValid || currentPageResult.UseGeneratedViewModel)
            {
                return View(currentPageResult.ViewName, currentPageResult.ViewModel);
            }

            var behaviour = _pageService.GetBehaviour(currentPageResult);

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
                        form,
                        path
                    });
                case EBehaviourType.SubmitAndPay:
                    var result = await _paymentWorkflow.Submit(form, path);
                    return Redirect(result);
                default:
                    throw new ApplicationException($"The provided behaviour type '{behaviour.BehaviourType}' is not valid");
            }
        }

        [HttpGet]
        [Route("{form}/submit")]
        public async Task<IActionResult> Submit(string form)
        {
            var result = await _submitWorkflow.Submit(form);

            ViewData["BannerTypeformUrl"] = result.FeedbackFormUrl;
            return View(result.ViewName, result.ViewModel);
        }
    }
}