using form_builder.Enum;
using form_builder.Exceptions;
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
            if(id == Guid.Empty)
                return RedirectToAction("Index", "Error");
            
            try {
                _documentWorkflow.GenerateDocument(EDocumentContentType.Summary, documentType, id);
            } catch(DocumentExpiredException){
                return RedirectToAction("Expired");
            }

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
