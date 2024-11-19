using form_builder.Models;
using form_builder.Services.PageService.Entities;

namespace form_builder.Services.AddressService;

public interface IAddressService
{
    Task<ProcessRequestEntity> ProcessAddress(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string cacheKey,
        string path);
}