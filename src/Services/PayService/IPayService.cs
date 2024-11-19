using form_builder.Services.MappingService.Entities;

namespace form_builder.Services.PayService
{
    public interface IPayService
    {
        Task<string> ProcessPayment(MappingEntity mappingEntity, string form, string path, string reference, string cacheKey);
        Task<string> ProcessPaymentResponse(string form, string responseCode, string reference);
    }
}
