namespace form_builder.Services.EmailSubmitService;

public interface IEmailSubmitService
{
    Task<string> EmailSubmission(MappingEntity data, string form, string cacheKey);
}