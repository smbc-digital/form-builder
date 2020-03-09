using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using Microsoft.AspNetCore.Http;

namespace form_builder.Services.FileUploadService
{
    public interface IFileUploadService
    {
        Dictionary<string, dynamic> AddFiles(Dictionary<string, dynamic> viewModel, IFormFileCollection fileUpload);

        FormAnswers CollectAnswers(FormAnswers currentPageAnswers, IFormFileCollection files,
            Dictionary<string, dynamic> viewModel);
    }
}
