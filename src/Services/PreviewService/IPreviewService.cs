namespace form_builder.Services.PreviewService;

public interface IPreviewService
{
    Task<FormBuilderViewModel> GetPreviewPage();
    void ExitPreviewMode();
    Task<ProcessPreviewRequestEntity> VerifyPreviewRequest(IEnumerable<CustomFormFile> fileUpload);
}