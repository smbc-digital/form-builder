using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using form_builder.Models;


namespace form_builder.Helpers
{
    // Note: This class isn't easily testable.
    // System.IO.Abstractions nor SystemWrapper are available for .Net Core 1.0
    public class FileHelper
    {
      public static DocumentModel ConvertFileToBase64StringWithFileName(IFormFile file)
        {
            var documentFile = new DocumentModel();
            using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    string fileBaseEncoded = Convert.ToBase64String(fileBytes);
                    documentFile = new DocumentModel
                    {
                        Content = fileBaseEncoded,
                        FileName = file.FileName,
                        FileSize = file.Length
                    };                   
                }
            return documentFile;
        }
    }
}
