using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Models;

namespace form_builder.Providers.PaymentProvider
{
    public interface IPaymentProvider
    {
        string ProviderName { get; }

        Task<string> GeneratePaymentUrl(string form, string path, string reference, string sessionGuid, PaymentInformation paymentInformation, List<Models.Answers> formAnswers);

        void VerifyPaymentResponse(string responseCode);

        StreetData GetHouseNumber(string addressLine1);
    }

}
