using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Services.PageService.Entities;

namespace form_builder.Services.FileUploadService
{
    public interface IFileUploadService
    {
        Dictionary<string, dynamic> AddFiles(Dictionary<string, dynamic> viewModel, IEnumerable<CustomFormFile> fileUpload);

        ProcessRequestEntity RemoveFile(Dictionary<string, dynamic> viewModel, FormSchema baseForm, string path, string sessionGuid);

        Task<ProcessRequestEntity> ProcessFile(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path,
            IEnumerable<CustomFormFile> files);
    }
}
