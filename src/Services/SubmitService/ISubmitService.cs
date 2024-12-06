using form_builder.Services.MappingService.Entities;

namespace form_builder.Services.SubmitService;

public interface ISubmitService
{
    Task PreProcessSubmission(string form, string cacheKey);
    Task<string> ProcessSubmission(MappingEntity mappingEntity, string form, string cacheKey);
    Task<string> PaymentSubmission(MappingEntity mappingEntity, string form, string cacheKey);
    Task<string> RedirectSubmission(MappingEntity mappingEntity, string form, string cacheKey);
    Task<string> ProcessWithoutSubmission(MappingEntity mappingEntity, string form, string cacheKey);
}