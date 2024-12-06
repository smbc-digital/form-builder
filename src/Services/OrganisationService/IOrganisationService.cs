using form_builder.Models;
using form_builder.Services.PageService.Entities;

namespace form_builder.Services.OrganisationService;

public interface IOrganisationService
{
    Task<ProcessRequestEntity> ProcessOrganisation(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string cacheKey, string path);
}