using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Services.DocumentService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Summary(EDocumentType documentType, Guid id)
        {
            if(id == Guid.Empty)
                return RedirectToAction("Index", "Error");
            
            try {
                var result = await _documentWorkflow.GenerateDocument(EDocumentContentType.Summary, documentType, id);
                return File(result, documentType.ToContentType(), "summary.txt");
            } catch(DocumentExpiredException ex){
                _logger.LogWarning(ex.Message);
                return RedirectToAction("Expired");
            }
        }
        
        [HttpGet]
        [Route("expired")]
        public IActionResult Expired()
        {
            return View();
        }
    }
}
