using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Configuration;

namespace form_builder.Providers.PaymentProvider
{
    public interface IPaymentProvider
    {
        string ProviderName { get; }

        Task<string> GeneratePaymentUrl(string form, string path, string reference, string sessionGuid, PaymentInformation paymentInformation, List<Models.Answers> formAnswers);

        void VerifyPaymentResponse(string responseCode);

        Dictionary<string, string> GetHouseNumber(string addressLine1);
    }

}
