using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Services.PageService.Entities;

namespace form_builder.Services.AddAnotherService
{
    public interface IAddAnotherService
    {
        Page ReplaceAddAnotherWithElements(Page currentPage, bool addEmptyFieldset, string sessionGuid);

        Page GenerateAddAnotherElementsForValidation(Page currentPage, Dictionary<string, dynamic> viewModel);

        Task<ProcessRequestEntity> ProcessAddAnother(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path);
    }
}
