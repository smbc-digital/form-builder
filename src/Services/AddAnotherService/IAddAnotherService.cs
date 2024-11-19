using form_builder.Models;
using form_builder.Services.PageService.Entities;

namespace form_builder.Services.AddAnotherService;

public interface IAddAnotherService
{
    Task<ProcessRequestEntity> ProcessAddAnother(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string cacheKey,
        string path);
}