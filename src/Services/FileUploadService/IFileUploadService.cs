using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.Services.FileUploadService
{
    public interface IFileUploadService
    {
        Dictionary<string, dynamic> AddFiles(Dictionary<string, dynamic> viewModel, IEnumerable<CustomFormFile> fileUpload);
        List<Answers> SaveFormFileAnswers(List<Answers> answers, IEnumerable<CustomFormFile> files);
    }
}
