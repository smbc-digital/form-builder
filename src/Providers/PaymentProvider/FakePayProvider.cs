using form_builder.Configuration;
using form_builder.Exceptions;
using form_builder.Extensions;
using StockportGovUK.NetStandard.Gateways.CivicaPay;

namespace form_builder.Providers.PaymentProvider
{
    public class FakePayProvider : IPaymentProvider
    {
        public string ProviderName => "Fake";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;

        public FakePayProvider(ICivicaPayGateway civicaPayGateway, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, ILogger<CivicaPayProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }
        public async Task<string> GeneratePaymentUrl(string form, string path, string reference, string cacheKey, PaymentInformation paymentInformation)
        {
            string url = $"https://{_httpContextAccessor.HttpContext.Request.Host}{_environment.EnvironmentName.ToReturnUrlPrefix()}/{form}/{path}/fake-payment?reference={reference}&amount={paymentInformation.Settings.Amount}";
            return url;
        }

        public void VerifyPaymentResponse(string responseCode)
        {
            if (responseCode.Equals("00022") || responseCode.Equals("00023") || responseCode.Equals("00001"))
                throw new PaymentDeclinedException($"FakePayProvider::Declined payment with response code: {responseCode}");

            if (!responseCode.Equals("00000"))
                throw new PaymentFailureException($"FakePayProvider::Payment failed with response code: {responseCode}");
        }
    }
}