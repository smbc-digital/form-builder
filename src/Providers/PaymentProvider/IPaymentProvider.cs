using System.Threading.Tasks;
using form_builder.Configuration;

namespace form_builder.Providers.PaymentProvider
{
    public interface IPaymentProvider
    {
        string ProviderName { get; }
        Task<string> GeneratePaymentUrl(string form, string path, string reference, string sessionGuid, PaymentInformation paymentInformation);
        string VerifyPaymentResponse(string responseCode);
    }
}