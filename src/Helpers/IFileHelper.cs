using Microsoft.AspNetCore.Http;
using form_builder.Models;
using System;

namespace form_builder.Helpers
{
    public interface IFileHelper
    {
        DocumentModel ConvertFileToBase64StringWithFileName(IFormFile file);
    }
}
