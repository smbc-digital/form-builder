using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Services.DocumentService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Summary(EDocumentType documentType, Guid id)
        {
            if(id == Guid.Empty)
                return RedirectToAction("Index", "Error");
            
            try {
                var result = await _documentWorkflow.GenerateDocument(EDocumentContentType.Summary, documentType, id);
                return File(result, documentType.ToContentType(), "summary.txt");
            } catch(DocumentExpiredException){
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
