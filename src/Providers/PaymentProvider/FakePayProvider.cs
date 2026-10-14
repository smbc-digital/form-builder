namespace form_builder.Providers.PaymentProvider;

public class FakePayProvider(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment) : IPaymentProvider
{
    public string ProviderName => "Fake";

    public async Task<string> GeneratePaymentUrl(string form, string path, string reference, string cacheKey, PaymentInformation paymentInformation)
    {
        string url = $"https://{httpContextAccessor.HttpContext.Request.Host}{environment.EnvironmentName.ToReturnUrlPrefix()}/{form}/{path}/fake-payment?reference={reference}&amount={paymentInformation.Settings.Amount}";
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