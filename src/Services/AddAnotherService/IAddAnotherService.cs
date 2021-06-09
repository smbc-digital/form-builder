using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Services.PageService.Entities;
using form_builder.Validators;

namespace form_builder.Services.AddAnotherService
{
    public interface IAddAnotherService
    {
        (FormSchema dynamicFormSchema, Page dynamicCurrentPage) GetDynamicPageFromFormData(Page currentPage, string guid);

        Page GenerateAddAnotherElementsForValidation(Page currentPage, Dictionary<string, dynamic> viewModel);

        Task<ProcessRequestEntity> ProcessAddAnother(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path,
            IEnumerable<IElementValidator> validators);
    }
}
