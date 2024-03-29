﻿using form_builder.Constants;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Services.PreviewService;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers
{
    [Route("[Controller]")]
    public class PreviewController : Controller
    {
        private readonly IPreviewService _previewService;

        public PreviewController(IPreviewService previewService) => _previewService = previewService;

        [HttpGet]
        public async Task<IActionResult> Index()
            => View(await _previewService.GetPreviewPage());

        [HttpPost]
        public async Task<IActionResult> IndexPost(IEnumerable<CustomFormFile> fileUpload)
        {
            var currentPageResult = await _previewService.VerifyPreviewRequest(fileUpload);

            if (!currentPageResult.Page.IsValid || currentPageResult.UseGeneratedViewModel)
            {
                if (!currentPageResult.Page.IsValid)
                    currentPageResult.ViewModel.PageTitle = currentPageResult.ViewModel.PageTitle.ToStringWithPrefix(PageTitleConstants.VALIDATION_ERROR_PREFIX);

                return View("Index", currentPageResult.ViewModel);
            }

            return Redirect($"{currentPageResult.PreviewFormKey}");
        }

        [Route("exit")]
        [HttpGet]
        public IActionResult Exit()
        {
            _previewService.ExitPreviewMode();
            return View();
        }
    }
}