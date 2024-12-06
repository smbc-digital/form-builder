using form_builder.Models;
using form_builder.Services.PageService.Entities;

namespace form_builder.Services.FileUploadService;

public interface IFileUploadService
{
    Dictionary<string, dynamic> AddFiles(Dictionary<string, dynamic> viewModel, IEnumerable<CustomFormFile> fileUpload);

    Task<ProcessRequestEntity> ProcessFile(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string cacheKey,
        string path,
        IEnumerable<CustomFormFile> files,
        bool modelStateIsValid);
}