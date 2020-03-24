using form_builder.Enum;
using form_builder.Services.DocumentService;
using Microsoft.AspNetCore.Mvc;
using System;
namespace form_builder.Controllers.Document
{
    [Route("document")]
    public class DocumentController : Controller
    {
        private IDocumentWorkflow _documentWorkflow;
        public DocumentController(IDocumentWorkflow documentWorkflow)
        {
            _documentWorkflow = documentWorkflow;
        }

        [HttpGet]
        [Route("Summary/{documentType}/{id}")]
        public IActionResult Summary(EDocumentType documentType, Guid id)
        {
            _documentWorkflow.GenerateSummaryDocument();
            return Ok();
        }
        
        [HttpGet]
        [Route("expired")]
        public IActionResult Expired()
        {
            return View();
        }
    }
}
