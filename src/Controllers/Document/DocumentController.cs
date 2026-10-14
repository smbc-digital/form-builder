namespace form_builder.Controllers.Document;

[Route("document")]
public class DocumentController(IDocumentWorkflow documentWorkflow, ILogger<DocumentController> logger)
    : Controller
{
    [HttpGet]
    [Route("Summary/{documentType}/{id}")]
    public async Task<IActionResult> Summary(EDocumentType documentType, string id)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction("Index", "Error");

        try
        {
            var result = await documentWorkflow.GenerateSummaryDocumentAsync(documentType, id);

            return File(result, documentType.ToContentType(), $"summary.{documentType.ToString().ToLower()}");
        }
        catch (DocumentExpiredException ex)
        {
            logger.LogWarning(ex, ex.Message);

            return RedirectToAction("Expired");
        }
    }

    [HttpGet]
    [Route("expired")]
    public IActionResult Expired() => View();
}