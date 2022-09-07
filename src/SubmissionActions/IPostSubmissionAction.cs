using form_builder.Services.MappingService.Entities;

namespace form_builder.SubmissionActions
{
    public interface IPostSubmissionAction
    {
        Task ConfirmResult(MappingEntity mappingEntity, string environmentName);
    }
}
