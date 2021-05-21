using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using form_builder.Services.PreviewService;
using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.Controllers
{
    [Route("preview")]
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
                return View("Index", currentPageResult.ViewModel);

            return Redirect($"{currentPageResult.PreviewFormId}");
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