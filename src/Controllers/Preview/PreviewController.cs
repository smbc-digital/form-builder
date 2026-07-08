namespace form_builder.Controllers.Preview;

[Route("[Controller]")]
public class PreviewController(IPreviewService previewService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
        => View(await previewService.GetPreviewPage());

    [HttpPost]
    public async Task<IActionResult> IndexPost(IEnumerable<CustomFormFile> fileUpload)
    {
        var currentPageResult = await previewService.VerifyPreviewRequest(fileUpload);

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
        previewService.ExitPreviewMode();
        return View();
    }
}