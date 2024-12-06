using form_builder.Models;
using form_builder.Services.PageService.Entities;

namespace form_builder.Services.StreetService;

public interface IStreetService
{
    Task<ProcessRequestEntity> ProcessStreet(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string cacheKey, string path);
}