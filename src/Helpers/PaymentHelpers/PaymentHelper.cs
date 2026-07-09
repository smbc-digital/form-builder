namespace form_builder.Helpers.PaymentHelpers;

public class PaymentHelper(IGateway gateway,
    ISessionHelper sessionHelper,
    IMappingService mappingService,
    IWebHostEnvironment hostingEnvironment,
    IPaymentConfigurationTransformDataProvider paymentConfigProvider,
    IOptions<PaymentConfiguration> paymentConfiguration)
    : IPaymentHelper
{
    private readonly PaymentConfiguration _paymentConfiguration = paymentConfiguration.Value;

    public async Task<PaymentInformation> GetFormPaymentInformation(string formName, FormAnswers formAnswers, FormSchema baseForm)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string cacheKey = $"{formName}::{browserSessionId}";
        MappingEntity mappingEntity = await mappingService.Map(cacheKey, formName, formAnswers, baseForm);
        if (mappingEntity is null)
            throw new Exception($"PayService:: No mapping entity found for {formName}");

        List<PaymentInformation> paymentConfig = await paymentConfigProvider.Get<List<PaymentInformation>>();
        PaymentInformation formPaymentConfig = paymentConfig.FirstOrDefault(_ => _.FormName.Any(_ => _.Equals(formName)));

        if (formPaymentConfig is null)
            throw new Exception($"PayService:: No payment information found for {formName}");

        if (!string.IsNullOrEmpty(formPaymentConfig.Settings.AddressReference))
            formPaymentConfig.Settings.AddressReference = formPaymentConfig.Settings.AddressReference.Insert(formPaymentConfig.Settings.AddressReference.Length - 2, AddressConstants.DESCRIPTION_SUFFIX);

        if (string.IsNullOrEmpty(formPaymentConfig.Settings.Amount))
            formPaymentConfig.Settings.Amount = await GetPaymentAmountAsync(mappingEntity, formPaymentConfig);

        return formPaymentConfig;
    }

    private async Task<string> GetPaymentAmountAsync(MappingEntity formData, PaymentInformation formPaymentConfig)
    {
        if (_paymentConfiguration.FakePaymentCalculation)
            return "0.01";

        try
        {
            var postUrl = formPaymentConfig.Settings.CalculationSlug;

            if (postUrl.URL is null || postUrl.AuthToken is null)
                throw new Exception($"{nameof(PaymentHelper)}::{nameof(GetPaymentAmountAsync)}: slug for {hostingEnvironment.EnvironmentName} not found or incomplete");

            HttpResponseMessage response;

            if (postUrl.Type.Equals("flowtoken"))
            {
                response = await gateway.PostAsync(postUrl.URL, formData.Data, postUrl.Type, postUrl.AuthToken);
            }
            else
            {
                gateway.ChangeAuthenticationHeader(postUrl.AuthToken);
                response = await gateway.PostAsync(postUrl.URL, formData.Data);
            }

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{nameof(PaymentHelper)}::{nameof(GetPaymentAmountAsync)}: Gateway returned unsuccessful status code {response.StatusCode}; Request: {JsonConvert.SerializeObject(formData.Data)}; Response: {JsonConvert.SerializeObject(response)}");

            if (response.Content is null)
                throw new ApplicationException($"{nameof(PaymentHelper)}::{nameof(GetPaymentAmountAsync)}: Gateway {postUrl.URL} responded with null content");

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                throw new ApplicationException($"{nameof(PaymentHelper)}::{nameof(GetPaymentAmountAsync)}: Gateway {postUrl.URL} responded with empty payment amount within content");

            return JsonConvert.DeserializeObject<string>(content);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}