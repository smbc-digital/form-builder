using System.Threading.Tasks;
using form_builder.Services.MappingService.Entities;

namespace form_builder.Services.SubmitService
{
    public interface ISubmitService
    {
        Task PreProcessSubmission(string form, string sessionGuid);
        Task<string> ProcessSubmission(MappingEntity mappingEntity, string form, string sessionGuid);
        Task<string> PaymentSubmission(MappingEntity mappingEntity, string form, string sessionGuid);
        Task<string> RedirectSubmission(MappingEntity mappingEntity, string form, string sessionGuid);
    }
}
