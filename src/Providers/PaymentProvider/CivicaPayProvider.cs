namespace form_builder.Providers.PaymentProvider;

public class CivicaPayProvider(ICivicaPayGateway civicaPayGateway,
    IOptions<CivicaPaymentConfiguration> paymentConfiguration,
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment environment)
    : IPaymentProvider
{
    public string ProviderName => "CivicaPay";
    private readonly CivicaPaymentConfiguration _paymentConfig = paymentConfiguration.Value;

    public async Task<string> GeneratePaymentUrl(string form, string path, string reference, string cacheKey, PaymentInformation paymentInformation)
    {
        if (string.IsNullOrEmpty(reference))
            throw new PaymentFailureException("CivicaPayProvider::No valid reference");

        var address = !string.IsNullOrEmpty(paymentInformation.Settings.AddressReference) ? paymentInformation.Settings.AddressReference.ConvertStringToObject() : new AddressDetail();

        CreateImmediateBasketRequest basket = new()
        {
            CallingAppIdentifier = "Basket",
            CustomerID = _paymentConfig.CustomerId,
            ApiPassword = _paymentConfig.ApiPassword,
            ReturnURL = $"https://{httpContextAccessor.HttpContext.Request.Host}{environment.EnvironmentName.ToReturnUrlPrefix()}/{form}/{path}/payment-response",
            NotifyURL = string.Empty,
            CallingAppTranReference = reference,
            PaymentItems = new List<PaymentItem>
            {
                new PaymentItem
                {
                    PaymentDetails = new PaymentDetail
                    {
                        CatalogueID = paymentInformation.Settings.CatalogueId,
                        AccountReference = paymentInformation.Settings.AccountReference,
                        PaymentAmount = paymentInformation.Settings.Amount,
                        Quantity = "1",
                        PaymentNarrative = form,
                        CallingAppTranReference = reference,
                        ServicePayItemDesc = paymentInformation.Settings.Description
                    },
                    AddressDetails = new AddressDetail
                    {
                        HouseNo = address.HouseNo,
                        Street = address.Street,
                        Town = address.Town,
                        Postcode = address.Postcode
                    }
                }
            }
        };

        if (paymentInformation.IsServicePay())
        {
            basket.PaymentItems[0].PaymentDetails.ServicePayReference = paymentInformation.Settings.ServicePayReference;
            basket.PaymentItems[0].PaymentDetails.ServicePayNarrative = paymentInformation.Settings.ServicePayNarrative;
        }

        if (!string.IsNullOrEmpty(paymentInformation.Settings.Name))
            basket.PaymentItems[0].AddressDetails.Name = paymentInformation.Settings.Name;

        if (!string.IsNullOrEmpty(paymentInformation.Settings.Email))
            basket.PaymentItems[0].PaymentDetails.EmailAddress = paymentInformation.Settings.Email;

        var civicaResponse = await civicaPayGateway.CreateImmediateBasketAsync(basket);

        if (!civicaResponse.IsSuccessStatusCode)
            throw new Exception($"CivicaPayProvider::GeneratePaymentUrl, CivicaPay gateway response with a non ok status code {civicaResponse.StatusCode}, HttpResponse: {JsonConvert.SerializeObject(civicaResponse)}");

        if (!civicaResponse.ResponseContent.Success)
        {
            throw new Exception($"CivicaPayProvider::GeneratePaymentUrl, CivicaPay gateway responded with a non successful response from the provider, {civicaResponse.ResponseContent.ResponseCode}, Summary: {civicaResponse.ResponseContent.ErrorMessage} - {civicaResponse.ResponseContent.ErrorSummary}, HttpResponse: {JsonConvert.SerializeObject(civicaResponse)}");
        }

        return civicaPayGateway.GetPaymentUrl(civicaResponse.ResponseContent.BasketReference, civicaResponse.ResponseContent.BasketToken, reference);
    }

    public void VerifyPaymentResponse(string responseCode)
    {
        if (responseCode.Equals("00022") || responseCode.Equals("00023") || responseCode.Equals("00001"))
            throw new PaymentDeclinedException($"CivicaPayProvider::Declined payment with response code: {responseCode}");

        if (!responseCode.Equals("00000"))
            throw new PaymentFailureException($"CivicaPayProvider::Payment failed with response code: {responseCode}");
    }
}