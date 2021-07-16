using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Models;

namespace form_builder.Providers.PaymentProvider
{
    public interface IPaymentProvider
    {
        string ProviderName { get; }

        Task<string> GeneratePaymentUrl(string form, string path, string reference, string sessionGuid, PaymentInformation paymentInformation, FormAnswers formData);

        void VerifyPaymentResponse(string responseCode);
    }
}