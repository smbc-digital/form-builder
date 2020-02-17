using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using form_builder.Models;

namespace form_builder.Helpers
{
    public interface IFileHelper
    {
        
        DocumentModel ConvertFileToBase64StringWithFileName(IFormFile file);
    }
}
