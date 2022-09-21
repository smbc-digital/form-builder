using form_builder.Models;
using form_builder.Services.PageService.Entities;
using form_builder.ViewModels;

namespace form_builder.Services.PreviewService
{
    public interface IPreviewService
    {
        Task<FormBuilderViewModel> GetPreviewPage();
        void ExitPreviewMode();
        Task<ProcessPreviewRequestEntity> VerifyPreviewRequest(IEnumerable<CustomFormFile> fileUpload);
    }
}