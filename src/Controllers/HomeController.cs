using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using form_builder.Enum;
using System.Threading.Tasks;
using System;
using form_builder.Services.PageService;
using form_builder.Extensions;
using form_builder.Workflows;
using form_builder.Services.SubmitAndPayService;

namespace form_builder.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPageService _pageService;
        private readonly ISubmitWorkflow _submitWorkflow;
        private readonly ISubmitAndPayService _submitAndPayService;

        public HomeController(IPageService pageService, ISubmitWorkflow submitWorkflow, ISubmitAndPayService submitAndPayService)
        {
            _pageService = pageService;
            _submitWorkflow = submitWorkflow;
            _submitAndPayService = submitAndPayService;
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
                    return RedirectToAction("SubmitAndPay", new
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
            var result = await _submitWorkflow.Submit(form);

            ViewData["BannerTypeformUrl"] = result.FeedbackFormUrl;
            return View(result.ViewName, result.ViewModel);
        }

        [HttpGet]
        [Route("{form}/submitandpay")]
        public async Task<IActionResult> SubmitAndPay(string form)
        {
            var result = await _submitAndPayService.ProcessSubmission(form);

            var reference = ((form_builder.Models.Success)result.ViewModel).Reference;
            var catId = ((form_builder.Models.Success)result.ViewModel).FormAnswers.Pages[1].Answers[1].Response;
            var accRef = ((form_builder.Models.Success)result.ViewModel).FormAnswers.Pages[1].Answers[2].Response;
            var payAmount = ((form_builder.Models.Success)result.ViewModel).FormAnswers.Pages[1].Answers[3].Response;
            var path = ((Models.Success)result.ViewModel).FormAnswers.Path;

            return Redirect(await _submitAndPayService.GeneratePaymentUrl(reference, form, path, catId, accRef, payAmount));
        }

        [HttpGet]
        [Route("{form}/{path}/payment-response")]
        public async Task<IActionResult> HandlePaymentResponse(string form, string path, [FromQuery]string responseCode, [FromQuery]string callingAppTxnRef)
        {
            if (responseCode != "00000")
            {
                throw new Exception("Payment failed");
            }

            return RedirectToAction("Success", new
            {
                path
            });
        }
    }
}