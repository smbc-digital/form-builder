using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Workflows.DocumentWorkflow;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers.Document
{
	[Route("document")]
	public class DocumentController : Controller
	{
		private IDocumentWorkflow _documentWorkflow;
		private ILogger<DocumentController> _logger;

		public DocumentController(IDocumentWorkflow documentWorkflow, ILogger<DocumentController> logger)
		{
			_documentWorkflow = documentWorkflow;
			_logger = logger;
		}

		[HttpGet]
		[Route("Summary/{documentType}/{id}")]
		public async Task<IActionResult> Summary(EDocumentType documentType, string id)
		{
			if (string.IsNullOrEmpty(id)) return RedirectToAction("Index", "Error");

			try
			{
				var result = await _documentWorkflow.GenerateSummaryDocumentAsync(documentType, id);

				return File(result, documentType.ToContentType(), "summary.txt");
			}
			catch (DocumentExpiredException ex)
			{
				_logger.LogWarning(ex, ex.Message);

				return RedirectToAction("Expired");
			}
		}

		[HttpGet]
		[Route("expired")]
		public IActionResult Expired() => View();
	}
}